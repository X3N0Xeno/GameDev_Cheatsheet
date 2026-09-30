using System.Collections.Generic;
using GameDevCheatsheet.Models;

namespace GameDevCheatsheet.Data.Categories;

public static class UiUxCategory
{
    public static ComponentCategory GetCategory()
    {
        return new ComponentCategory
        {
            Name = "UI/UX COMPONENTS",
            Icon = "🩺",
            IsExpanded = true,
            Items = new List<ComponentItem>
            {
                new ComponentItem
                {
                    Id = "linear-bar",
                    Title = "Linear Health Bar (Smooth..",
                    Category = "UI/UX COMPONENTS",
                    SubRoute = "Linear Health Bar (Smooth Fill)",
                    ScriptFilename = "smooth_health_bar.gd",
                    GDScript = @"extends Control

@export var max_health: int = 100
var current_health: int = 70

@onready var health_bar: TextureProgressBar = $HealthBar
@onready var ghost_bar: TextureProgressBar = $GhostBar

var health_tween: Tween
var ghost_tween: Tween

func _ready() -> void:
    health_bar.max_value = max_health
    ghost_bar.max_value = max_health
    health_bar.value = current_health
    ghost_bar.value = current_health

func take_damage(amount: int) -> void:
    current_health = clamp(current_health - amount, 0, max_health)
    
    if health_tween and health_tween.is_valid(): health_tween.kill()
    health_tween = create_tween().set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
    health_tween.tween_property(health_bar, ""value"", float(current_health), 0.2)
    
    if ghost_tween and ghost_tween.is_valid(): ghost_tween.kill()
    ghost_tween = create_tween().set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
    ghost_tween.tween_interval(0.8)
    ghost_tween.tween_property(ghost_bar, ""value"", float(current_health), 0.5)

func heal(amount: int) -> void:
    current_health = clamp(current_health + amount, 0, max_health)
    
    if health_tween and health_tween.is_valid(): health_tween.kill()
    if ghost_tween and ghost_tween.is_valid(): ghost_tween.kill()
    
    health_tween = create_tween().set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
    health_tween.tween_property(health_bar, ""value"", float(current_health), 0.3)
    ghost_bar.value = current_health",
                    CSharpScript = @"using Godot;

public partial class SmoothHealthBar : Control
{
    [Export] public int MaxHealth = 100;
    public int CurrentHealth = 70;

    private TextureProgressBar _healthBar;
    private TextureProgressBar _ghostBar;
    private Tween _healthTween;
    private Tween _ghostTween;

    public override void _Ready()
    {
        _healthBar = GetNode<TextureProgressBar>(""HealthBar"");
        _ghostBar = GetNode<TextureProgressBar>(""GhostBar"");
        _healthBar.MaxValue = MaxHealth;
        _ghostBar.MaxValue = MaxHealth;
        _healthBar.Value = CurrentHealth;
        _ghostBar.Value = CurrentHealth;
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, MaxHealth);

        if (_healthTween != null && _healthTween.IsValid()) _healthTween.Kill();
        _healthTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _healthTween.TweenProperty(_healthBar, ""value"", (double)CurrentHealth, 0.2);

        if (_ghostTween != null && _ghostTween.IsValid()) _ghostTween.Kill();
        _ghostTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _ghostTween.TweenInterval(0.8);
        _ghostTween.TweenProperty(_ghostBar, ""value"", (double)CurrentHealth, 0.5);
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);

        if (_healthTween != null && _healthTween.IsValid()) _healthTween.Kill();
        if (_ghostTween != null && _ghostTween.IsValid()) _ghostTween.Kill();

        _healthTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _healthTween.TweenProperty(_healthBar, ""value"", (double)CurrentHealth, 0.3);
        _ghostBar.Value = CurrentHealth;
    }
}",
                    SceneTreeGuide = @"CanvasLayer
 └─ HealthBarUI (Control)
     ├─ GhostBar (TextureProgressBar)
     │   fill_mode: 0 (LEFT_TO_RIGHT)
     │   modulate: Color(1, 0.85, 0, 1)
     │   z_index: 0
     ├─ HealthBar (TextureProgressBar)
     │   fill_mode: 0 (LEFT_TO_RIGHT)
     │   modulate: Color(0.9, 0.1, 0.1, 1)
     │   z_index: 1
     └─ HealthLabel (Label)",
                    SetupSteps = new List<string>
                    {
                        "Create a CanvasLayer node in your UI scene.",
                        "Add a Control node named HealthBarUI and attach smooth_health_bar.gd.",
                        "Add two TextureProgressBar child nodes: GhostBar and HealthBar.",
                        "Assign matching textures to under and progress fields for both bars.",
                        "Set modulate colors: Yellow for GhostBar and Red for HealthBar.",
                        "Call take_damage(amount) or heal(amount) when damage events occur."
                    }
                },
                new ComponentItem
                {
                    Id = "heart-system",
                    Title = "Segmented Heart System",
                    Category = "UI/UX COMPONENTS",
                    SubRoute = "Segmented Heart System",
                    ScriptFilename = "heart_container_system.gd",
                    GDScript = @"extends HBoxContainer

@export var max_hearts: int = 5
@export var full_heart_texture: Texture2D
@export var half_heart_texture: Texture2D
@export var empty_heart_texture: Texture2D

var current_health: int = 7
var heart_icons: Array[TextureRect] = []

func _ready() -> void:
    for child in get_children(): child.queue_free()
    heart_icons.clear()
    
    for i in max_hearts:
        var rect = TextureRect.new()
        rect.expand_mode = TextureRect.EXPAND_KEEP_SIZE
        rect.stretch_mode = TextureRect.STRETCH_KEEP_CENTER
        add_child(rect)
        heart_icons.append(rect)
        
    update_hearts()

func take_damage(amount: int) -> void:
    current_health = clamp(current_health - amount, 0, max_hearts * 2)
    update_hearts()

func heal(amount: int) -> void:
    current_health = clamp(current_health + amount, 0, max_hearts * 2)
    update_hearts()

func update_hearts() -> void:
    for i in max_hearts:
        var heart_val = current_health - (i * 2)
        if heart_val >= 2:
            heart_icons[i].texture = full_heart_texture
        elif heart_val == 1:
            heart_icons[i].texture = half_heart_texture
        else:
            heart_icons[i].texture = empty_heart_texture",
                    CSharpScript = @"using Godot;
using System.Collections.Generic;

public partial class HeartContainerSystem : HBoxContainer
{
    [Export] public int MaxHearts = 5;
    [Export] public Texture2D FullHeartTexture;
    [Export] public Texture2D HalfHeartTexture;
    [Export] public Texture2D EmptyHeartTexture;

    public int CurrentHealth = 7;
    private readonly List<TextureRect> _heartIcons = new();

    public override void _Ready()
    {
        foreach (Node child in GetChildren()) child.QueueFree();
        _heartIcons.Clear();

        for (int i = 0; i < MaxHearts; i++)
        {
            var rect = new TextureRect
            {
                ExpandMode = TextureRect.ExpandModeEnum.KeepSize,
                StretchMode = TextureRect.StretchModeEnum.KeepCenter
            };
            AddChild(rect);
            _heartIcons.Add(rect);
        }
        UpdateHearts();
    }

    public void TakeDamage(int halfHearts)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - halfHearts, 0, MaxHearts * 2);
        UpdateHearts();
    }

    public void Heal(int halfHearts)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + halfHearts, 0, MaxHearts * 2);
        UpdateHearts();
    }

    public void UpdateHearts()
    {
        for (int i = 0; i < MaxHearts; i++)
        {
            int heartVal = CurrentHealth - (i * 2);
            if (heartVal >= 2)
                _heartIcons[i].Texture = FullHeartTexture;
            else if (heartVal == 1)
                _heartIcons[i].Texture = HalfHeartTexture;
            else
                _heartIcons[i].Texture = EmptyHeartTexture;
        }
    }
}",
                    SceneTreeGuide = @"CanvasLayer
 └─ HeartContainerSystem (HBoxContainer)
     ├─ Theme Overrides / Constants / Separation: 8px
     ├─ Script: heart_container_system.gd
     └─ Export Textures:
         ├─ FullHeartTexture
         ├─ HalfHeartTexture
         └─ EmptyHeartTexture",
                    SetupSteps = new List<string>
                    {
                        "Create an HBoxContainer in your HUD and attach the script.",
                        "In Inspector, assign Full, Half, and Empty heart sprite textures.",
                        "Set separation in Theme Overrides > Constants > Separation.",
                        "The script auto-instantiates TextureRect children in _ready().",
                        "Call take_damage(1) for half heart, or take_damage(2) for full heart."
                    }
                },
                new ComponentItem
                {
                    Id = "overhead-bar",
                    Title = "Floating Overhead Bar",
                    Category = "UI/UX COMPONENTS",
                    SubRoute = "Floating Overhead Bar",
                    ScriptFilename = "floating_overhead_bar.gd",
                    GDScript = @"extends Node2D

@export var max_health: int = 100
@export var follow_offset: Vector2 = Vector2(0, -50)
var current_health: int = 100

@onready var health_bar: TextureProgressBar = $ProgressBar
@onready var entity: Node2D = get_parent() as Node2D

func _ready() -> void:
    top_level = true
    health_bar.max_value = max_health
    health_bar.value = current_health

func _process(_delta: float) -> void:
    if is_instance_valid(entity):
        global_position = entity.global_position + follow_offset

func take_damage(amount: int) -> void:
    current_health = clamp(current_health - amount, 0, max_health)
    var tween = create_tween().set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
    tween.tween_property(health_bar, ""value"", float(current_health), 0.2)
    if current_health <= 0:
        hide()",
                    CSharpScript = @"using Godot;

public partial class FloatingOverheadBar : Node2D
{
    [Export] public int MaxHealth = 100;
    [Export] public Vector2 FollowOffset = new Vector2(0, -50);
    public int CurrentHealth = 100;

    private TextureProgressBar _healthBar;
    private Node2D _entity;

    public override void _Ready()
    {
        TopLevel = true;
        _healthBar = GetNode<TextureProgressBar>(""ProgressBar"");
        _entity = GetParent<Node2D>();
        _healthBar.MaxValue = MaxHealth;
        _healthBar.Value = CurrentHealth;
    }

    public override void _Process(double delta)
    {
        if (IsInstanceValid(_entity))
        {
            GlobalPosition = _entity.GlobalPosition + FollowOffset;
        }
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, MaxHealth);
        var tween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_healthBar, ""value"", (double)CurrentHealth, 0.2);
        if (CurrentHealth <= 0) Hide();
    }
}",
                    SceneTreeGuide = @"CharacterBody2D (or Enemy Node2D)
 ├─ AnimatedSprite2D / Sprite2D
 ├─ CollisionShape2D
 └─ OverheadUI (Node2D - with script attached)
     └─ ProgressBar (TextureProgressBar)",
                    SetupSteps = new List<string>
                    {
                        "Add a Node2D named OverheadUI as a child of your Enemy or Entity.",
                        "Attach floating_overhead_bar.gd to OverheadUI.",
                        "Add a TextureProgressBar child named ProgressBar.",
                        "Set TopLevel = true (the script sets this automatically in _ready).",
                        "Adjust follow_offset in inspector to position above sprite head."
                    }
                },
                new ComponentItem
                {
                    Id = "shield-layer",
                    Title = "Shield & Armor Layer",
                    Category = "UI/UX COMPONENTS",
                    SubRoute = "Shield & Armor Layer",
                    ScriptFilename = "shield_armor_system.gd",
                    GDScript = @"extends Control

@export var max_shield: float = 50.0
@export var max_health: float = 100.0
@export var shield_regen_delay: float = 4.0
@export var shield_regen_rate: float = 12.0

var shield: float = 50.0
var health: float = 100.0
var time_since_last_hit: float = 0.0

@onready var shield_bar: TextureProgressBar = $ShieldBar
@onready var health_bar: TextureProgressBar = $HealthBar

func _ready() -> void:
    shield_bar.max_value = max_shield
    health_bar.max_value = max_health
    _update_bars()

func _process(delta: float) -> void:
    time_since_last_hit += delta
    if time_since_last_hit >= shield_regen_delay and shield < max_shield:
        shield = move_toward(shield, max_shield, shield_regen_rate * delta)
        shield_bar.value = shield

func take_damage(amount: float) -> void:
    time_since_last_hit = 0.0
    if shield > 0.0:
        var overflow = amount - shield
        shield = max(0.0, shield - amount)
        if overflow > 0.0:
            health = max(0.0, health - overflow)
    else:
        health = max(0.0, health - amount)
    _update_bars()

func heal_health(amount: float) -> void:
    health = min(max_health, health + amount)
    _update_bars()

func _update_bars() -> void:
    var tween = create_tween().set_parallel(true).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
    tween.tween_property(shield_bar, ""value"", shield, 0.15)
    tween.tween_property(health_bar, ""value"", health, 0.15)",
                    CSharpScript = @"using Godot;

public partial class ShieldArmorSystem : Control
{
    [Export] public float MaxShield = 50.0f;
    [Export] public float MaxHealth = 100.0f;
    [Export] public float ShieldRegenDelay = 4.0f;
    [Export] public float ShieldRegenRate = 12.0f;

    public float Shield = 50.0f;
    public float Health = 100.0f;
    private float _timeSinceLastHit = 0.0f;

    private TextureProgressBar _shieldBar;
    private TextureProgressBar _healthBar;

    public override void _Ready()
    {
        _shieldBar = GetNode<TextureProgressBar>(""ShieldBar"");
        _healthBar = GetNode<TextureProgressBar>(""HealthBar"");
        _shieldBar.MaxValue = MaxShield;
        _healthBar.MaxValue = MaxHealth;
        UpdateBars();
    }

    public override void _Process(double delta)
    {
        _timeSinceLastHit += (float)delta;
        if (_timeSinceLastHit >= ShieldRegenDelay && Shield < MaxShield)
        {
            Shield = Mathf.MoveToward(Shield, MaxShield, ShieldRegenRate * (float)delta);
            _shieldBar.Value = Shield;
        }
    }

    public void TakeDamage(float amount)
    {
        _timeSinceLastHit = 0.0f;
        if (Shield > 0.0f)
        {
            float overflow = amount - Shield;
            Shield = Mathf.Max(0.0f, Shield - amount);
            if (overflow > 0.0f) Health = Mathf.Max(0.0f, Health - overflow);
        }
        else
        {
            Health = Mathf.Max(0.0f, Health - amount);
        }
        UpdateBars();
    }

    public void HealHealth(float amount)
    {
        Health = Mathf.Min(MaxHealth, Health + amount);
        UpdateBars();
    }

    private void UpdateBars()
    {
        var tween = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_shieldBar, ""value"", Shield, 0.15);
        tween.TweenProperty(_healthBar, ""value"", Health, 0.15);
    }
}",
                    SceneTreeGuide = @"CanvasLayer
 └─ ShieldHealthUI (Control)
     ├─ ShieldBar (TextureProgressBar)
     │   modulate: Color(0, 0.8, 1, 1)
     │   z_index: 1
     └─ HealthBar (TextureProgressBar)
         modulate: Color(0.9, 0.1, 0.1, 1)
         z_index: 0",
                    SetupSteps = new List<string>
                    {
                        "Create a Control node named ShieldHealthUI and attach shield_armor_system.gd.",
                        "Add two TextureProgressBar child nodes: ShieldBar and HealthBar.",
                        "Assign Cyan tint to ShieldBar and Red tint to HealthBar.",
                        "When player takes a hit, call take_damage(amount).",
                        "Health potions/healing items should call heal_health(amount)."
                    }
                }
            }
        };
    }
}