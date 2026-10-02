using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.Json;
using GameDevCheatsheet.Models;
using GameDevCheatsheet.Data;
using GameDevCheatsheet.Components.Previews;

namespace GameDevCheatsheet.Pages;

public partial class Index : ComponentBase
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private List<ComponentCategory> categories = new();
    private ComponentItem selectedComponent = new();
    private string activeTab = "gdscript";
    private string copyText = "COPY";

    // Playable Component References
    private PlatformerPreview? platformerRef;
    private TopDownPreview? topdownRef;
    private FightingPreview? fighterRef;
    private FpsPreview? fpsRef;
    private ThirdPersonPreview? tpRef;
    private HitscanPreview? hitscanRef;
    private ProjectilePreview? projectileRef;
    private MeleePreview? meleeRef;
    private InventoryPreview? inventoryRef;
    private InteractablePreview? interactRef;
    private DayNightPreview? dayNightRef;

    // Linear Health Bar
    private int targetHealth = 70;
    private int maxHealth = 100;
    private double displayHealth = 70.0;
    private double displayGhost = 70.0;
    private bool enableGhost = true;
    private int linearSession = 0;

    // Heart System (Max range: 5 to 30)
    private int heartPoints = 10;
    private int maxHearts = 5;

    // Overhead Bar
    private int overheadHealth = 100;

    // Shield & Armor
    private float actualShield = 50f;
    private float actualShieldHealth = 100f;
    private double displayShield = 50.0;
    private double displayShieldHealth = 100.0;
    private bool isShieldBroken = false;
    private int shieldCooldown = 0;
    private int damageHitSession = 0;
    private int healthHealSession = 0;

    // Menus & Settings
    private int mainMenuIdx = 0;
    private string menuStatus = "STANDBY";
    private bool isGamePaused = true;

    private float masterVol = 0.8f;
    private float musicVol = 0.65f;
    private float sfxVol = 0.9f;
    private bool masterMute = false;
    private bool musicMute = false;
    private bool sfxMute = false;

    private bool settingFullscreen = true;
    private bool settingVsync = true;
    private bool settingScreenShake = true;
    private bool settingFps = false;
    private bool settingCrt = true;

    // HUD State
    private int hudHealth = 100;
    private int hudScore = 48200;
    private float hudStamina = 85f;
    private int hudCurrentAmmo = 24;
    private int hudReserveAmmo = 30;
    private bool hudFlash = false;
    private int staminaSession = 0;

    // Search & Sidebar Filtering
    private string searchQuery = "";

    protected override void OnInitialized()
    {
        categories = ComponentRepository.GetCategories();
        selectedComponent = categories[0].Items[0];
    }

    private void SelectComponent(ComponentItem item)
    {
        selectedComponent = item;
    }

    // Health logic
    private async Task TriggerHealthDelta(int delta)
    {
        targetHealth = Math.Clamp(targetHealth + delta, 0, maxHealth);
        int session = ++linearSession;

        if (displayHealth < targetHealth)
        {
            while (displayHealth < targetHealth && session == linearSession)
            {
                displayHealth = Math.Min(targetHealth, displayHealth + 1.2);
                displayGhost = displayHealth;
                StateHasChanged();
                await Task.Delay(16);
            }
        }
        else if (displayHealth > targetHealth)
        {
            while (displayHealth > targetHealth && session == linearSession)
            {
                displayHealth = Math.Max(targetHealth, displayHealth - 1.8);
                StateHasChanged();
                await Task.Delay(16);
            }

            if (!enableGhost) { displayGhost = displayHealth; StateHasChanged(); return; }
            await Task.Delay(800);

            while (displayGhost > targetHealth && session == linearSession)
            {
                displayGhost = Math.Max(targetHealth, displayGhost - 0.7);
                StateHasChanged();
                await Task.Delay(16);
            }
        }
    }

    private async Task OnSliderInput(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int val))
            await TriggerHealthDelta(val - targetHealth);
    }

    // Heart System adjustments (5 to 30 hearts bounds)
    private void IncreaseMaxHearts()
    {
        if (maxHearts < 30)
        {
            maxHearts++;
            heartPoints = Math.Min(maxHearts * 2, heartPoints + 2);
        }
    }

    private void DecreaseMaxHearts()
    {
        if (maxHearts > 5)
        {
            maxHearts--;
            heartPoints = Math.Min(maxHearts * 2, heartPoints);
        }
    }

    private void ResetHearts()
    {
        maxHearts = 5;
        heartPoints = 10;
    }

    // Shield logic
    private async Task DamageShield()
    {
        float damage = 20f;
        int thisDamageSession = ++damageHitSession;
        isShieldBroken = true;
        shieldCooldown = 4000;

        if (actualShield > 0)
        {
            float overflow = damage - actualShield;
            actualShield = Math.Max(0f, actualShield - damage);
            if (overflow > 0) actualShieldHealth = Math.Max(0f, actualShieldHealth - overflow);
        }
        else
        {
            actualShieldHealth = Math.Max(0f, actualShieldHealth - damage);
        }

        while ((displayShield > actualShield || displayShieldHealth > actualShieldHealth) && thisDamageSession == damageHitSession)
        {
            if (displayShield > actualShield) displayShield = Math.Max(actualShield, displayShield - 1.5);
            if (displayShieldHealth > actualShieldHealth) displayShieldHealth = Math.Max(actualShieldHealth, displayShieldHealth - 1.5);
            StateHasChanged();
            await Task.Delay(16);
        }

        while (shieldCooldown > 0 && thisDamageSession == damageHitSession)
        {
            await Task.Delay(100);
            shieldCooldown -= 100;
            StateHasChanged();
        }

        if (thisDamageSession == damageHitSession)
        {
            actualShield = 50f;
            while (displayShield < actualShield && thisDamageSession == damageHitSession)
            {
                displayShield = Math.Min(actualShield, displayShield + 0.6);
                StateHasChanged();
                await Task.Delay(16);
            }
            isShieldBroken = false;
        }
    }

    private async Task HealShieldHealth()
    {
        actualShieldHealth = Math.Min(100f, actualShieldHealth + 20f);
        int thisHealSession = ++healthHealSession;

        while (displayShieldHealth < actualShieldHealth && thisHealSession == healthHealSession)
        {
            displayShieldHealth = Math.Min(actualShieldHealth, displayShieldHealth + 1.2);
            StateHasChanged();
            await Task.Delay(16);
        }
    }

    private void ResetShield()
    {
        actualShield = 50f;
        actualShieldHealth = 100f;
        displayShield = 50.0;
        displayShieldHealth = 100.0;
        isShieldBroken = false;
        shieldCooldown = 0;
    }

    // Main Menu logic
    private void OnMainMenuOptionClicked(int idx)
    {
        mainMenuIdx = idx;
        menuStatus = idx switch
        {
            0 => "NEW GAME INITIALIZED",
            1 => "LOAD ARCHIVES READY",
            2 => "SETTINGS OPEN",
            3 => "APPLICATION TERMINATED",
            _ => "STANDBY"
        };
    }

    private void TriggerMenuAction(string action)
    {
        menuStatus = action;
        mainMenuIdx = action switch
        {
            "NEW GAME" => 0,
            "LOAD GAME" => 1,
            "SETTINGS" => 2,
            "QUIT" => 3,
            _ => 0
        };
    }

    // HUD logic
    private async Task TakeHudHit()
    {
        hudHealth = Math.Max(0, hudHealth - 20);
        hudFlash = true;
        StateHasChanged();
        await Task.Delay(200);
        hudFlash = false;
        StateHasChanged();
    }

    private void FireWeapon()
    {
        if (hudCurrentAmmo > 0) hudCurrentAmmo--;
    }

    private async Task ConsumeHudStamina()
    {
        hudStamina = Math.Max(0f, hudStamina - 25f);
        int curSession = ++staminaSession;
        StateHasChanged();

        await Task.Delay(1200);
        while (hudStamina < 100f && curSession == staminaSession)
        {
            hudStamina = Math.Min(100f, hudStamina + 1.5f);
            StateHasChanged();
            await Task.Delay(20);
        }
    }

    private void ResetHud()
    {
        hudHealth = 100;
        hudScore = 48200;
        hudStamina = 85f;
        hudCurrentAmmo = 24;
        hudReserveAmmo = 30;
    }

    // Interactable helpers
    private void MoveInteractLeft() { interactRef?.MoveLeft(); StateHasChanged(); }
    private void MoveInteractRight() { interactRef?.MoveRight(); StateHasChanged(); }
    private void TriggerInteract() { interactRef?.Interact(); StateHasChanged(); }
    private void ResetInteract() { interactRef?.Reset(); StateHasChanged(); }

    // Projectile & Inventory helpers
    private async Task TriggerRapidFire()
    {
        for (int i = 0; i < 3; i++)
        {
            projectileRef?.SpawnProjectile();
            await Task.Delay(140);
        }
    }

    private async Task AddFiveItems()
    {
        for (int i = 0; i < 5; i++)
        {
            inventoryRef?.AddItem();
            await Task.Delay(40);
        }
    }

    // Unified Global Keyboard Handler
    private void HandleGlobalKeydown(KeyboardEventArgs e)
    {
        if (e.Key.Equals("Escape", StringComparison.OrdinalIgnoreCase))
        {
            isGamePaused = !isGamePaused;
            StateHasChanged();
            return;
        }

        if (selectedComponent.Id == "main-menu")
        {
            if (e.Key.Equals("w", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowUp", StringComparison.OrdinalIgnoreCase))
            {
                mainMenuIdx = (mainMenuIdx - 1 + 4) % 4;
                OnMainMenuOptionClicked(mainMenuIdx);
                StateHasChanged();
                return;
            }
            else if (e.Key.Equals("s", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowDown", StringComparison.OrdinalIgnoreCase))
            {
                mainMenuIdx = (mainMenuIdx + 1) % 4;
                OnMainMenuOptionClicked(mainMenuIdx);
                StateHasChanged();
                return;
            }
            else if (e.Key.Equals("Enter", StringComparison.OrdinalIgnoreCase))
            {
                OnMainMenuOptionClicked(mainMenuIdx);
                StateHasChanged();
                return;
            }
        }

        switch (selectedComponent.Id)
        {
            case "interact-prompt":
                if (e.Key.Equals("e", StringComparison.OrdinalIgnoreCase)) { interactRef?.Interact(); StateHasChanged(); }
                else if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowLeft", StringComparison.OrdinalIgnoreCase)) { interactRef?.MoveLeft(); StateHasChanged(); }
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowRight", StringComparison.OrdinalIgnoreCase)) { interactRef?.MoveRight(); StateHasChanged(); }
                break;

            case "fighting-2d":
                if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase)) fighterRef?.MoveLeft(true);
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase)) fighterRef?.MoveRight(true);
                else if (e.Key.Equals("j", StringComparison.OrdinalIgnoreCase)) fighterRef?.P1Poke("LIGHT");
                else if (e.Key.Equals("k", StringComparison.OrdinalIgnoreCase)) fighterRef?.P1Poke("HEAVY");
                else if (e.Key.Equals("s", StringComparison.OrdinalIgnoreCase)) fighterRef?.P1Crouch();
                break;

            case "fps-3d":
                if (e.Key.Equals("w", StringComparison.OrdinalIgnoreCase)) fpsRef!.KeyW = true;
                else if (e.Key.Equals("s", StringComparison.OrdinalIgnoreCase)) fpsRef!.KeyS = true;
                else if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase)) fpsRef!.KeyA = true;
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase)) fpsRef!.KeyD = true;
                else if (e.Key.Equals(" ")) fpsRef?.Jump();
                break;

            case "thirdperson-3d":
                if (e.Key.Equals("w", StringComparison.OrdinalIgnoreCase)) tpRef!.KeyW = true;
                else if (e.Key.Equals("s", StringComparison.OrdinalIgnoreCase)) tpRef!.KeyS = true;
                else if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase)) tpRef!.KeyA = true;
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase)) tpRef!.KeyD = true;
                else if (e.Key.Equals("q", StringComparison.OrdinalIgnoreCase)) tpRef?.Orbit(-20);
                else if (e.Key.Equals("e", StringComparison.OrdinalIgnoreCase)) tpRef?.Orbit(20);
                break;

            case "platformer-2d":
                if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowLeft", StringComparison.OrdinalIgnoreCase)) platformerRef?.MoveLeft(true);
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowRight", StringComparison.OrdinalIgnoreCase)) platformerRef?.MoveRight(true);
                else if (e.Key.Equals(" ") || e.Key.Equals("ArrowUp", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("w", StringComparison.OrdinalIgnoreCase)) platformerRef?.TriggerJump();
                break;

            case "topdown-2d":
                if (e.Key.Equals("w", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowUp", StringComparison.OrdinalIgnoreCase)) topdownRef?.MoveUp(true);
                else if (e.Key.Equals("s", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowDown", StringComparison.OrdinalIgnoreCase)) topdownRef?.MoveDown(true);
                else if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowLeft", StringComparison.OrdinalIgnoreCase)) topdownRef?.MoveLeft(true);
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowRight", StringComparison.OrdinalIgnoreCase)) topdownRef?.MoveRight(true);
                break;

            case "melee-hitbox":
                if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase)) meleeRef?.MoveLeft(true);
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase)) meleeRef?.MoveRight(true);
                else if (e.Key.Equals("j", StringComparison.OrdinalIgnoreCase)) meleeRef?.Attack("LIGHT");
                else if (e.Key.Equals("k", StringComparison.OrdinalIgnoreCase)) meleeRef?.Attack("HEAVY");
                else if (e.Key.Equals(" ")) meleeRef?.Jump();
                break;
        }
    }

    private void HandleGlobalKeyup(KeyboardEventArgs e)
    {
        switch (selectedComponent.Id)
        {
            case "fighting-2d":
                if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase)) fighterRef?.MoveLeft(false);
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase)) fighterRef?.MoveRight(false);
                else if (e.Key.Equals("s", StringComparison.OrdinalIgnoreCase)) fighterRef?.P1Neutral();
                break;

            case "fps-3d":
                if (e.Key.Equals("w", StringComparison.OrdinalIgnoreCase)) fpsRef!.KeyW = false;
                else if (e.Key.Equals("s", StringComparison.OrdinalIgnoreCase)) fpsRef!.KeyS = false;
                else if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase)) fpsRef!.KeyA = false;
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase)) fpsRef!.KeyD = false;
                break;

            case "thirdperson-3d":
                if (e.Key.Equals("w", StringComparison.OrdinalIgnoreCase)) tpRef!.KeyW = false;
                else if (e.Key.Equals("s", StringComparison.OrdinalIgnoreCase)) tpRef!.KeyS = false;
                else if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase)) tpRef!.KeyA = false;
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase)) tpRef!.KeyD = false;
                break;

            case "platformer-2d":
                if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowLeft", StringComparison.OrdinalIgnoreCase)) platformerRef?.MoveLeft(false);
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowRight", StringComparison.OrdinalIgnoreCase)) platformerRef?.MoveRight(false);
                break;

            case "topdown-2d":
                if (e.Key.Equals("w", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowUp", StringComparison.OrdinalIgnoreCase)) topdownRef?.MoveUp(false);
                else if (e.Key.Equals("s", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowDown", StringComparison.OrdinalIgnoreCase)) topdownRef?.MoveDown(false);
                else if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowLeft", StringComparison.OrdinalIgnoreCase)) topdownRef?.MoveLeft(false);
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase) || e.Key.Equals("ArrowRight", StringComparison.OrdinalIgnoreCase)) topdownRef?.MoveRight(false);
                break;

            case "melee-hitbox":
                if (e.Key.Equals("a", StringComparison.OrdinalIgnoreCase)) meleeRef?.MoveLeft(false);
                else if (e.Key.Equals("d", StringComparison.OrdinalIgnoreCase)) meleeRef?.MoveRight(false);
                break;
        }
    }

    private async Task CopyCode()
    {
        string rawCode = activeTab switch
        {
            "csharp" => selectedComponent.CSharpScript,
            "guide" => $"{selectedComponent.SceneTreeGuide}\n\nSETUP STEPS:\n" + string.Join("\n", selectedComponent.SetupSteps),
            _ => selectedComponent.GDScript
        };

        try
        {
            await JS.InvokeVoidAsync("eval", $"navigator.clipboard.writeText({JsonSerializer.Serialize(rawCode)})");
            copyText = "COPIED!";
            StateHasChanged();
            await Task.Delay(1400);
            copyText = "COPY";
            StateHasChanged();
        }
        catch
        {
            copyText = "FAILED!";
            StateHasChanged();
            await Task.Delay(1400);
            copyText = "COPY";
            StateHasChanged();
        }
    }

    private void HandleSearchInput(ChangeEventArgs e)
    {
        searchQuery = e.Value?.ToString() ?? "";
    }

    private void ClearSearch()
    {
        searchQuery = "";
    }

    private void SetAllCategories(bool expanded)
    {
        foreach (var c in categories) c.IsExpanded = expanded;
    }

    private List<ComponentCategory> GetFilteredCategories()
    {
        if (string.IsNullOrWhiteSpace(searchQuery)) return categories;

        var term = searchQuery.Trim();
        var results = new List<ComponentCategory>();

        foreach (var cat in categories)
        {
            var matchingItems = cat.Items
                .Where(i => i.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            i.SubRoute.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            i.Category.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingItems.Any())
            {
                results.Add(new ComponentCategory
                {
                    Name = cat.Name,
                    Icon = cat.Icon,
                    IsExpanded = true,
                    Items = matchingItems
                });
            }
        }
        return results;
    }
}