using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.Json;

namespace GameDevCheatsheet.Pages;

public partial class Sandbox : ComponentBase, IDisposable
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private enum ToolMode { Platform, Hazard, Coin, Erase }
    private ToolMode currentTool = ToolMode.Platform;

    public record PlatformBlock(double X, double Y, double W, double H);
    public record HazardBlock(double X, double Y);
    public record CoinItem(double X, double Y);

    private List<PlatformBlock> platforms = new();
    private List<HazardBlock> hazards = new();
    private List<CoinItem> coins = new();

    private double playerX = 100;
    private double playerY = 380;
    private double playerVx = 0;
    private double playerVy = 0;
    private int playerFacing = 1;
    private bool onFloor = false;
    private bool playerFlash = false;
    private int score = 0;
    private int deaths = 0;

    private double speed = 260.0;
    private double jumpVelocity = 480.0;
    private double gravity = 1100.0;
    private double acceleration = 1800.0;
    private double friction = 1400.0;

    private bool keyLeft = false;
    private bool keyRight = false;
    private bool jumpBuffered = false;

    private CancellationTokenSource cts = new();

    public class SvgPointResult
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    protected override void OnInitialized()
    {
        LoadDefaultLayout();
        _ = RunGameLoopAsync(cts.Token);
    }

    private void LoadDefaultLayout()
    {
        platforms = new List<PlatformBlock>
        {
            new(60, 360, 140, 16),
            new(260, 300, 120, 16),
            new(440, 240, 140, 16),
            new(640, 180, 120, 16),
            new(320, 140, 100, 16)
        };

        hazards = new List<HazardBlock>
        {
            new(290, 440),
            new(330, 440),
            new(500, 440)
        };

        coins = new List<CoinItem>
        {
            new(130, 330),
            new(320, 270),
            new(510, 210),
            new(700, 150),
            new(370, 110)
        };

        RespawnPlayer();
    }

    private void ClearMap()
    {
        platforms.Clear();
        hazards.Clear();
        coins.Clear();
    }

    private void RespawnPlayer()
    {
        playerX = 90;
        playerY = 320;
        playerVx = 0;
        playerVy = 0;
    }

    private async Task HandleCanvasClick(MouseEventArgs e)
    {
        double targetSvgX;
        double targetSvgY;

        try
        {
            // Precise CTM matrix transformation from window client coords to SVG local space
            var pt = await JS.InvokeAsync<SvgPointResult>("eval", 
                $@"(() => {{
                    const svg = document.getElementById('sandboxSvgCanvas');
                    if (!svg) return {{ x: {e.ClientX}, y: {e.ClientY} }};
                    let point = svg.createSVGPoint();
                    point.x = {e.ClientX};
                    point.y = {e.ClientY};
                    let ctm = svg.getScreenCTM();
                    if (!ctm) return {{ x: {e.ClientX}, y: {e.ClientY} }};
                    let transformed = point.matrixTransform(ctm.inverse());
                    return {{ x: transformed.x, y: transformed.y }};
                }})()");

            targetSvgX = pt.X;
            targetSvgY = pt.Y;
        }
        catch
        {
            targetSvgX = e.OffsetX;
            targetSvgY = e.OffsetY;
        }

        // Snap precisely to 20px grid
        double clickX = Math.Round(targetSvgX / 20.0) * 20.0;
        double clickY = Math.Round(targetSvgY / 20.0) * 20.0;

        clickX = Math.Clamp(clickX, 30, 830);
        clickY = Math.Clamp(clickY, 30, 450);

        switch (currentTool)
        {
            case ToolMode.Platform:
                platforms.Add(new PlatformBlock(clickX - 40, clickY, 80, 16));
                break;
            case ToolMode.Hazard:
                hazards.Add(new HazardBlock(clickX - 15, clickY - 20));
                break;
            case ToolMode.Coin:
                coins.Add(new CoinItem(clickX, clickY));
                break;
            case ToolMode.Erase:
                platforms.RemoveAll(p => Math.Abs(p.X + p.W / 2 - clickX) < 55 && Math.Abs(p.Y - clickY) < 30);
                hazards.RemoveAll(h => Math.Abs(h.X + 15 - clickX) < 30 && Math.Abs(h.Y + 10 - clickY) < 30);
                coins.RemoveAll(c => Math.Abs(c.X - clickX) < 25 && Math.Abs(c.Y - clickY) < 25);
                break;
        }

        StateHasChanged();
    }

    private async Task RunGameLoopAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(16));
        DateTime lastTime = DateTime.UtcNow;

        while (!token.IsCancellationRequested && await timer.WaitForNextTickAsync(token))
        {
            DateTime now = DateTime.UtcNow;
            double dt = Math.Min((now - lastTime).TotalSeconds, 0.05);
            lastTime = now;

            double targetVx = 0;
            if (keyLeft) { targetVx = -speed; playerFacing = -1; }
            if (keyRight) { targetVx = speed; playerFacing = 1; }

            if (targetVx != 0)
                playerVx = Math.Clamp(playerVx + Math.Sign(targetVx - playerVx) * acceleration * dt, -speed, speed);
            else
                playerVx = Math.Sign(playerVx) * Math.Max(0, Math.Abs(playerVx) - friction * dt);

            playerVy += gravity * dt;

            if (jumpBuffered && onFloor)
            {
                playerVy = -jumpVelocity;
                jumpBuffered = false;
                onFloor = false;
            }

            playerX = Math.Clamp(playerX + playerVx * dt, 12, 848);
            playerY += playerVy * dt;

            onFloor = false;
            if (playerY >= 460)
            {
                playerY = 460;
                playerVy = 0;
                onFloor = true;
            }

            foreach (var p in platforms)
            {
                if (playerX + 8 > p.X && playerX - 8 < p.X + p.W &&
                    playerY >= p.Y && playerY <= p.Y + 14 && playerVy >= 0)
                {
                    playerY = p.Y;
                    playerVy = 0;
                    onFloor = true;
                }
            }

            foreach (var h in hazards)
            {
                if (Math.Abs(playerX - (h.X + 15)) < 16 && Math.Abs(playerY - (h.Y + 10)) < 16)
                {
                    deaths++;
                    playerFlash = true;
                    RespawnPlayer();
                    _ = Task.Delay(200).ContinueWith(_ => { playerFlash = false; InvokeAsync(StateHasChanged); });
                    break;
                }
            }

            for (int i = coins.Count - 1; i >= 0; i--)
            {
                var c = coins[i];
                if (Math.Abs(playerX - c.X) < 18 && Math.Abs(playerY - 12 - c.Y) < 18)
                {
                    score += 100;
                    coins.RemoveAt(i);
                }
            }

            await InvokeAsync(StateHasChanged);
        }
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowLeft", StringComparison.OrdinalIgnoreCase))
            keyLeft = true;
        else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowRight", StringComparison.OrdinalIgnoreCase))
            keyRight = true;
        else if (e.Key.Equals(" ", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("w", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowUp", StringComparison.OrdinalIgnoreCase))
            jumpBuffered = true;
    }

    private void HandleKeyUp(KeyboardEventArgs e)
    {
        if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowLeft", StringComparison.OrdinalIgnoreCase))
            keyLeft = false;
        else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowRight", StringComparison.OrdinalIgnoreCase))
            keyRight = false;
        else if (e.Key.Equals(" ", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("w", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowUp", StringComparison.OrdinalIgnoreCase))
            jumpBuffered = false;
    }

    private async Task CopyGodotScript()
    {
        string script = $@"extends CharacterBody2D

const SPEED = {speed:F1}
const JUMP_VELOCITY = -{jumpVelocity:F1}
const ACCELERATION = {acceleration:F1}
const FRICTION = {friction:F1}

var gravity = {gravity:F1}

func _physics_process(delta: float) -> void:
    if not is_on_floor():
        velocity.y += gravity * delta

    if Input.is_action_just_pressed(""ui_accept"") and is_on_floor():
        velocity.y = JUMP_VELOCITY

    var direction := Input.get_axis(""ui_left"", ""ui_right"")
    if direction:
        velocity.x = move_toward(velocity.x, direction * SPEED, ACCELERATION * delta)
    else:
        velocity.x = move_toward(velocity.x, 0, FRICTION * delta)

    move_and_slide()";

        await JS.InvokeVoidAsync("eval", $"navigator.clipboard.writeText({JsonSerializer.Serialize(script)})");
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }
}