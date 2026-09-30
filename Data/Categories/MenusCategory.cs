using System.Collections.Generic;
using GameDevCheatsheet.Models;

namespace GameDevCheatsheet.Data.Categories;

public static class MenusCategory
{
    public static ComponentCategory GetCategory() => new()
    {
        Name = "GAME MENUS & UI STATES",
        Icon = "📋",
        IsExpanded = false,
        Items = new List<ComponentItem>
        {
            new()
            {
                Id = "main-menu",
                Title = "Main Menu Hub",
                Category = "GAME MENUS & UI STATES",
                SubRoute = "Main Menu Hub",
                ScriptFilename = "main_menu.gd",
                GDScript = @"extends Control

@export_file(""*.tscn"") var first_level_scene: String = ""res://scenes/levels/level_01.tscn""

@onready var start_btn: Button = %StartButton
@onready var options_btn: Button = %OptionsButton
@onready var quit_btn: Button = %QuitButton

func _ready() -> void:
    start_btn.pressed.connect(_on_start_pressed)
    options_btn.pressed.connect(_on_options_pressed)
    quit_btn.pressed.connect(_on_quit_pressed)
    start_btn.grab_focus()

func _on_start_pressed() -> void:
    get_tree().change_scene_to_file(first_level_scene)

func _on_options_pressed() -> void:
    # Switch to options menu or overlay
    pass

func _on_quit_pressed() -> void:
    get_tree().quit()",
                CSharpScript = @"using Godot;

public partial class MainMenu : Control
{
    [Export(PropertyHint.File, ""*.tscn"")]
    public string FirstLevelScene = ""res://scenes/levels/level_01.tscn"";

    private Button _startBtn;
    private Button _optionsBtn;
    private Button _quitBtn;

    public override void _Ready()
    {
        _startBtn = GetNode<Button>(""%StartButton"");
        _optionsBtn = GetNode<Button>(""%OptionsButton"");
        _quitBtn = GetNode<Button>(""%QuitButton"");

        _startBtn.Pressed += () => GetTree().ChangeSceneToFile(FirstLevelScene);
        _optionsBtn.Pressed += () => { /* Open Settings */ };
        _quitBtn.Pressed += () => GetTree().Quit();

        _startBtn.GrabFocus();
    }
}",
                SceneTreeGuide = @"MainMenu (Control - Anchor: Full Rect)
 ├─ Background (ColorRect / TextureRect)
 └─ CenterContainer (Anchor: Full Rect)
     └─ VBoxContainer (Separation: 12)
         ├─ TitleLabel (Label)
         ├─ StartButton (Button - Unique Name: %StartButton)
         ├─ OptionsButton (Button - Unique Name: %OptionsButton)
         └─ QuitButton (Button - Unique Name: %QuitButton)",
                SetupSteps = new List<string>
                {
                    "Attach main_menu.gd to the root Control node.",
                    "Set VBoxContainer alignment to Center.",
                    "Right-click buttons and check 'Access as Unique Name' for % selectors.",
                    "Assign your default target scene in first_level_scene.",
                    "Set StartButton focus mode to All for gamepad/keyboard navigation."
                }
            },
            new()
            {
                Id = "pause-menu",
                Title = "Pause Overlay",
                Category = "GAME MENUS & UI STATES",
                SubRoute = "Pause Overlay",
                ScriptFilename = "pause_overlay.gd",
                GDScript = @"extends CanvasLayer

@onready var resume_btn: Button = %ResumeButton
@onready var restart_btn: Button = %RestartButton
@onready var quit_btn: Button = %QuitButton

func _ready() -> void:
    # Process mode MUST be ALWAYS so UI can run while SceneTree is paused
    process_mode = Node.PROCESS_MODE_ALWAYS
    hide()
    
    resume_btn.pressed.connect(unpause)
    restart_btn.pressed.connect(_on_restart_pressed)
    quit_btn.pressed.connect(_on_quit_pressed)

func _unhandled_input(event: InputEvent) -> void:
    if event.is_action_pressed(""ui_cancel""): # ESC key or gamepad Start
        if get_tree().paused:
            unpause()
        else:
            pause()

func pause() -> void:
    get_tree().paused = true
    show()
    resume_btn.grab_focus()

func unpause() -> void:
    get_tree().paused = false
    hide()

func _on_restart_pressed() -> void:
    unpause()
    get_tree().reload_current_scene()

func _on_quit_pressed() -> void:
    unpause()
    get_tree().change_scene_to_file(""res://scenes/main_menu.tscn"")",
                CSharpScript = @"using Godot;

public partial class PauseOverlay : CanvasLayer
{
    private Button _resumeBtn;
    private Button _restartBtn;
    private Button _quitBtn;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Hide();

        _resumeBtn = GetNode<Button>(""%ResumeButton"");
        _restartBtn = GetNode<Button>(""%RestartButton"");
        _quitBtn = GetNode<Button>(""%QuitButton"");

        _resumeBtn.Pressed += Unpause;
        _restartBtn.Pressed += OnRestartPressed;
        _quitBtn.Pressed += OnQuitPressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(""ui_cancel""))
        {
            if (GetTree().Paused) Unpause();
            else Pause();
        }
    }

    public void Pause()
    {
        GetTree().Paused = true;
        Show();
        _resumeBtn.GrabFocus();
    }

    public void Unpause()
    {
        GetTree().Paused = false;
        Hide();
    }

    private void OnRestartPressed()
    {
        Unpause();
        GetTree().ReloadCurrentScene();
    }

    private void OnQuitPressed()
    {
        Unpause();
        GetTree().ChangeSceneToFile(""res://scenes/main_menu.tscn"");
    }
}",
                SceneTreeGuide = @"PauseMenu (CanvasLayer - process_mode: ALWAYS)
 └─ Dimmer (ColorRect - Color: #00000080, Anchor: Full Rect)
     └─ CenterContainer (Anchor: Full Rect)
         └─ PanelContainer
             └─ VBoxContainer (Separation: 10)
                 ├─ PauseTitle (Label)
                 ├─ ResumeButton (Button - Unique Name: %ResumeButton)
                 ├─ RestartButton (Button - Unique Name: %RestartButton)
                 └─ QuitButton (Button - Unique Name: %QuitButton)",
                SetupSteps = new List<string>
                {
                    "Set PauseMenu node process_mode to Node.PROCESS_MODE_ALWAYS.",
                    "Ensure gameplay nodes use Node.PROCESS_MODE_PAUSABLE (default).",
                    "Add ColorRect with semi-transparent black color to dim gameplay.",
                    "Input action 'ui_cancel' toggles get_tree().paused state."
                }
            },
            new()
            {
                Id = "audio-settings",
                Title = "Settings / Audio Mixer",
                Category = "GAME MENUS & UI STATES",
                SubRoute = "Settings / Audio Mixer",
                ScriptFilename = "settings_menu.gd",
                GDScript = @"extends Control

# Display Checkboxes
@onready var fullscreen_check: CheckBox = %FullscreenCheck
@onready var vsync_check: CheckBox = %VsyncCheck

# Audio Sliders
@onready var master_slider: HSlider = %MasterSlider
@onready var music_slider: HSlider = %MusicSlider
@onready var sfx_slider: HSlider = %SFXSlider

var master_idx: int
var music_idx: int
var sfx_idx: int

func _ready() -> void:
    master_idx = AudioServer.get_bus_index(""Master"")
    music_idx = AudioServer.get_bus_index(""Music"")
    sfx_idx = AudioServer.get_bus_index(""SFX"")

    fullscreen_check.toggled.connect(_on_fullscreen_toggled)
    vsync_check.toggled.connect(_on_vsync_toggled)

    master_slider.value_changed.connect(func(v): _set_bus_vol(master_idx, v))
    music_slider.value_changed.connect(func(v): _set_bus_vol(music_idx, v))
    sfx_slider.value_changed.connect(func(v): _set_bus_vol(sfx_idx, v))

func _on_fullscreen_toggled(is_enabled: bool) -> void:
    var mode = DisplayServer.WINDOW_MODE_FULLSCREEN if is_enabled else DisplayServer.WINDOW_MODE_WINDOWED
    DisplayServer.window_set_mode(mode)

func _on_vsync_toggled(is_enabled: bool) -> void:
    var vsync = DisplayServer.VSYNC_MODE_ENABLED if is_enabled else DisplayServer.VSYNC_MODE_DISABLED
    DisplayServer.window_set_vsync_mode(vsync)

func _set_bus_vol(bus_idx: int, linear_val: float) -> void:
    if linear_val <= 0.0001:
        AudioServer.set_bus_mute(bus_idx, true)
    else:
        AudioServer.set_bus_mute(bus_idx, false)
        AudioServer.set_bus_volume_db(bus_idx, linear_to_db(linear_val))",
                CSharpScript = @"using Godot;

public partial class SettingsMenu : Control
{
    private CheckBox _fullscreenCheck;
    private CheckBox _vsyncCheck;
    private Slider _masterSlider;
    private Slider _musicSlider;
    private Slider _sfxSlider;

    private int _masterIdx;
    private int _musicIdx;
    private int _sfxIdx;

    public override void _Ready()
    {
        _masterIdx = AudioServer.GetBusIndex(""Master"");
        _musicIdx = AudioServer.GetBusIndex(""Music"");
        _sfxIdx = AudioServer.GetBusIndex(""SFX"");

        _fullscreenCheck = GetNode<CheckBox>(""%FullscreenCheck"");
        _vsyncCheck = GetNode<CheckBox>(""%VsyncCheck"");
        _masterSlider = GetNode<Slider>(""%MasterSlider"");
        _musicSlider = GetNode<Slider>(""%MusicSlider"");
        _sfxSlider = GetNode<Slider>(""%SFXSlider"");

        _fullscreenCheck.Toggled += OnFullscreenToggled;
        _vsyncCheck.Toggled += OnVsyncToggled;

        _masterSlider.ValueChanged += (v) => SetBusVol(_masterIdx, (float)v);
        _musicSlider.ValueChanged += (v) => SetBusVol(_musicIdx, (float)v);
        _sfxSlider.ValueChanged += (v) => SetBusVol(_sfxIdx, (float)v);
    }

    private void OnFullscreenToggled(bool isEnabled)
    {
        var mode = isEnabled ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
        DisplayServer.WindowSetMode(mode);
    }

    private void OnVsyncToggled(bool isEnabled)
    {
        var vsync = isEnabled ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled;
        DisplayServer.WindowSetVsyncMode(vsync);
    }

    private void SetBusVol(int busIdx, float linearVal)
    {
        if (linearVal <= 0.0001f)
        {
            AudioServer.SetBusMute(busIdx, true);
        }
        else
        {
            AudioServer.SetBusMute(busIdx, false);
            AudioServer.SetBusVolumeDb(busIdx, Mathf.LinearToDb(linearVal));
        }
    }
}",
                SceneTreeGuide = @"SettingsMenu (Control)
 └─ HBoxContainer (Separation: 24)
     ├─ DisplaySettings (VBoxContainer)
     │   ├─ Header (Label: ""DISPLAY PRESETS"")
     │   ├─ FullscreenCheck (CheckBox: %FullscreenCheck)
     │   └─ VsyncCheck (CheckBox: %VsyncCheck)
     └─ AudioMixer (HBoxContainer)
         ├─ MasterBus (VBoxContainer -> VSlider: %MasterSlider)
         ├─ MusicBus (VBoxContainer -> VSlider: %MusicSlider)
         └─ SFXBus (VBoxContainer -> VSlider: %SFXSlider)",
                SetupSteps = new List<string>
                {
                    "Configure Audio Buses in Godot bottom dock: add 'Music' and 'SFX' beside 'Master'.",
                    "Set VSlider min_value = 0.0, max_value = 1.0, step = 0.01.",
                    "Always convert slider linear values to dB with linear_to_db().",
                    "Handle window display changes via DisplayServer.window_set_mode."
                }
            },
            new()
            {
                Id = "hud-overlay",
                Title = "HUD Overlay",
                Category = "GAME MENUS & UI STATES",
                SubRoute = "HUD Overlay",
                ScriptFilename = "hud_overlay.gd",
                GDScript = @"extends CanvasLayer

@export var max_health: int = 100
@export var max_stamina: float = 100.0
@export var stamina_regen_delay: float = 1.5
@export var stamina_regen_rate: float = 40.0

var current_health: int = 100
var current_stamina: float = 100.0
var time_since_sprint: float = 0.0

@onready var health_bar: ProgressBar = %HealthBar
@onready var stamina_bar: ProgressBar = %StaminaBar
@onready var ammo_label: Label = %AmmoLabel
@onready var score_label: Label = %ScoreLabel
@onready var damage_rect: ColorRect = %DamageFlashRect

func _ready() -> void:
    health_bar.max_value = max_health
    health_bar.value = current_health
    stamina_bar.max_value = max_stamina
    stamina_bar.value = current_stamina
    damage_rect.modulate.a = 0.0

func _process(delta: float) -> void:
    time_since_sprint += delta
    if time_since_sprint >= stamina_regen_delay and current_stamina < max_stamina:
        current_stamina = move_toward(current_stamina, max_stamina, stamina_regen_rate * delta)
        stamina_bar.value = current_stamina

func take_damage(amount: int) -> void:
    current_health = max(0, current_health - amount)
    health_bar.value = current_health
    _flash_damage()

func use_stamina(amount: float) -> void:
    time_since_sprint = 0.0
    current_stamina = max(0.0, current_stamina - amount)
    stamina_bar.value = current_stamina

func update_ammo(current: int, reserve: int) -> void:
    ammo_label.text = ""%02d / %02d"" % [current, reserve]

func _flash_damage() -> void:
    var tween = create_tween()
    damage_rect.modulate.a = 0.4
    tween.tween_property(damage_rect, ""modulate:a"", 0.0, 0.3)",
                CSharpScript = @"using Godot;

public partial class HudOverlay : CanvasLayer
{
    [Export] public int MaxHealth = 100;
    [Export] public float MaxStamina = 100f;
    [Export] public float StaminaRegenDelay = 1.5f;
    [Export] public float StaminaRegenRate = 40f;

    public int CurrentHealth = 100;
    public float CurrentStamina = 100f;
    private float _timeSinceSprint = 0f;

    private ProgressBar _healthBar;
    private ProgressBar _staminaBar;
    private Label _ammoLabel;
    private ColorRect _damageRect;

    public override void _Ready()
    {
        _healthBar = GetNode<ProgressBar>(""%HealthBar"");
        _staminaBar = GetNode<ProgressBar>(""%StaminaBar"");
        _ammoLabel = GetNode<Label>(""%AmmoLabel"");
        _damageRect = GetNode<ColorRect>(""%DamageFlashRect"");

        _healthBar.MaxValue = MaxHealth;
        _healthBar.Value = CurrentHealth;
        _staminaBar.MaxValue = MaxStamina;
        _staminaBar.Value = CurrentStamina;
        _damageRect.Modulate = new Color(1, 1, 1, 0);
    }

    public override void _Process(double delta)
    {
        _timeSinceSprint += (float)delta;
        if (_timeSinceSprint >= StaminaRegenDelay && CurrentStamina < MaxStamina)
        {
            CurrentStamina = Mathf.MoveToward(CurrentStamina, MaxStamina, StaminaRegenRate * (float)delta);
            _staminaBar.Value = CurrentStamina;
        }
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        _healthBar.Value = CurrentHealth;

        var tween = CreateTween();
        _damageRect.Modulate = new Color(1, 1, 1, 0.4f);
        tween.TweenProperty(_damageRect, ""modulate:a"", 0.0f, 0.3f);
    }

    public void UseStamina(float amount)
    {
        _timeSinceSprint = 0f;
        CurrentStamina = Mathf.Max(0f, CurrentStamina - amount);
        _staminaBar.Value = CurrentStamina;
    }

    public void UpdateAmmo(int current, int reserve)
    {
        _ammoLabel.Text = $""{current:D2} / {reserve:D2}"";
    }
}",
                SceneTreeGuide = @"HUD (CanvasLayer - Layer: 10)
 ├─ DamageFlashRect (ColorRect - Color: Red, Modulate.a: 0, Anchor: Full Rect)
 ├─ TopRight (MarginContainer - Anchor: Top Right)
 │   └─ ScoreLabel (Label: %ScoreLabel)
 ├─ BottomRight (MarginContainer - Anchor: Bottom Right)
 │   └─ AmmoCounter (Label: %AmmoLabel)
 └─ BottomLeft (MarginContainer - Anchor: Bottom Left)
     └─ VBoxContainer
         ├─ HealthBar (ProgressBar: %HealthBar)
         └─ StaminaBar (ProgressBar: %StaminaBar)",
                SetupSteps = new List<string>
                {
                    "Set CanvasLayer layer index to 10 so it renders on top of the world.",
                    "Add ColorRect with red tint and modulate alpha at 0 for hit flash effects.",
                    "Use MarginContainer with corner anchors to pin elements dynamically across window sizes.",
                    "Regenerate stamina inside _process() using move_toward after delay expires."
                }
            }
        }
    };
}