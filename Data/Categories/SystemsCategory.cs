using System.Collections.Generic;
using GameDevCheatsheet.Models;

namespace GameDevCheatsheet.Data.Categories;

public static class SystemsCategory
{
    public static ComponentCategory GetCategory() => new()
    {
        Name = "SYSTEMS & UTILITY DRIVERS",
        Icon = "🎒",
        IsExpanded = false,
        Items = new List<ComponentItem>
        {
            new()
            {
                Id = "grid-inventory",
                Title = "Grid Inventory System",
                Category = "SYSTEMS & UTILITY DRIVERS",
                SubRoute = "Grid Inventory System",
                ScriptFilename = "grid_inventory.gd",
                GDScript = @"extends Control

# Drag & Drop Grid Inventory Management
@export var columns: int = 4
@export var max_slots: int = 16

var slots: Array[Dictionary] = []
@onready var grid: GridContainer = $GridContainer

func _ready() -> void:
    grid.columns = columns
    slots.resize(max_slots)
    for i in max_slots:
        slots[i] = { ""id"": """", ""count"": 0 }

# Godot 4 Drag & Drop Virtual Methods
func _get_drag_data_fw(at_position: Vector2, slot_index: int) -> Variant:
    if slots[slot_index].id == """": return null
    var preview = TextureRect.new()
    preview.texture = get_slot_icon(slot_index)
    preview.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
    preview.custom_minimum_size = Vector2(48, 48)
    set_drag_preview(preview)
    return { ""from_slot"": slot_index }

func _can_drop_data_fw(at_position: Vector2, data: Variant, slot_index: int) -> bool:
    return typeof(data) == TYPE_DICTIONARY and data.has(""from_slot"")

func _drop_data_fw(at_position: Vector2, data: Variant, target_slot: int) -> void:
    var from_slot = data[""from_slot""]
    # Swap or move into empty slot
    var temp = slots[target_slot]
    slots[target_slot] = slots[from_slot]
    slots[from_slot] = temp
    update_all_slots()",
                CSharpScript = @"using Godot;
using System.Collections.Generic;

public partial class GridInventory : Control
{
    [Export] public int Columns = 4;
    [Export] public int MaxSlots = 16;

    public class SlotItem { public string Id = """"; public int Count = 0; }
    public List<SlotItem> Slots = new();

    public override void _Ready()
    {
        for (int i = 0; i < MaxSlots; i++) Slots.Add(new SlotItem());
    }

    public void MoveOrSwapSlot(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;
        var temp = Slots[toIndex];
        Slots[toIndex] = Slots[fromIndex];
        Slots[fromIndex] = temp;
    }
}",
                SceneTreeGuide = @"Inventory (Control)
 └─ PanelContainer
     └─ GridContainer (Columns: 4)
         └─ SlotPanels [0..15] (PanelContainer: drag_data implemented)",
                SetupSteps = new List<string>
                {
                    "Attach script to Inventory root Control node.",
                    "Implement Godot 4 _get_drag_data(), _can_drop_data(), and _drop_data() on each slot panel.",
                    "Swapping slots permits true non-stacking inventory placement and organization."
                }
            },
            new()
            {
                Id = "interact-prompt",
                Title = "Interactable Object Prompt",
                Category = "SYSTEMS & UTILITY DRIVERS",
                SubRoute = "Interactable Object Prompt",
                ScriptFilename = "interactable_object.gd",
                GDScript = @"extends Area3D

@export var prompt_text: String = ""[ E ] INTERACT""
@onready var prompt_label: Label3D = $PromptLabel

func _ready() -> void:
    prompt_label.text = prompt_text
    prompt_label.hide()
    body_entered.connect(func(b): if b.is_in_group(""player""): prompt_label.show())
    body_exited.connect(func(b): if b.is_in_group(""player""): prompt_label.hide())

func _unhandled_input(event: InputEvent) -> void:
    if event.is_action_just_pressed(""interact"") and prompt_label.visible:
        execute_interaction()

func execute_interaction() -> void:
    print(""Triggered interaction on: "", name)",
                CSharpScript = @"using Godot;

public partial class InteractableObject : Area3D
{
    [Export] public string PromptText = ""[ E ] INTERACT"";
    private Label3D _label;

    public override void _Ready()
    {
        _label = GetNode<Label3D>(""PromptLabel"");
        _label.Text = PromptText;
        _label.Hide();

        BodyEntered += (b) => { if (b.IsInGroup(""player"")) _label.Show(); };
        BodyExited += (b) => { if (b.IsInGroup(""player"")) _label.Hide(); };
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionJustPressed(""interact"") && _label.Visible)
            ExecuteInteraction();
    }

    public virtual void ExecuteInteraction()
    {
        GD.Print(""Interacted with: "", Name);
    }
}",
                SceneTreeGuide = @"Interactable_Chest (Area3D - CollisionLayer: 16)
 ├─ CollisionShape3D (SphereShape3D: Radius = 2.5m)
 └─ PromptLabel (Label3D - Billboard: Enabled)",
                SetupSteps = new List<string>
                {
                    "Attach script to Area3D.",
                    "Enable Billboard = Enabled on Label3D so the prompt faces the camera.",
                    "Assign player entity to group 'player'.",
                    "Listen for Input.is_action_just_pressed('interact') when player is inside the radius."
                }
            },
            new()
            {
                Id = "day-night",
                Title = "Day / Night Cycle",
                Category = "SYSTEMS & UTILITY DRIVERS",
                SubRoute = "Day / Night Cycle",
                ScriptFilename = "day_night_fuzzy.gd",
                GDScript = @"extends DirectionalLight3D

# Continuous Fuzzy Membership Set Interpolation
@export var day_seconds: float = 60.0
@export var current_time: float = 0.25
@export var speed_multiplier: float = 1.0

# Fuzzy Reference Colors (Night, Dawn, Noon, Dusk)
const C_NIGHT := Color(0.05, 0.08, 0.2)
const C_DAWN  := Color(0.85, 0.45, 0.2)
const C_NOON  := Color(1.0, 0.96, 0.88)
const C_DUSK  := Color(0.75, 0.3, 0.15)

func _process(delta: float) -> void:
    current_time = fmod(current_time + (delta / day_seconds) * speed_multiplier, 1.0)
    
    # Sun celestial pitch
    rotation_degrees.x = (current_time * 360.0) - 90.0

    # 1. Fuzzy Membership Calculations
    var u_dawn := maxf(0.0, 1.0 - absf(current_time - 0.25) / 0.14)
    var u_noon := maxf(0.0, 1.0 - absf(current_time - 0.50) / 0.16)
    var u_dusk := maxf(0.0, 1.0 - absf(current_time - 0.75) / 0.14)
    var u_night := maxf(0.0, 1.0 - (current_time if current_time < 0.5 else 1.0 - current_time) / 0.22)
    var total_u := u_night + u_dawn + u_noon + u_dusk

    # 2. Defuzzification via Weighted Centroid
    light_color = (C_NIGHT * u_night + C_DAWN * u_dawn + C_NOON * u_noon + C_DUSK * u_dusk) / total_u
    light_energy = clamp((u_noon * 1.2 + u_dawn * 0.7 + u_dusk * 0.7), 0.05, 1.2)",
                CSharpScript = @"using Godot;

public partial class DayNightFuzzy : DirectionalLight3D
{
    [Export] public float DaySeconds = 60.0f;
    [Export] public float CurrentTime = 0.25f;
    [Export] public float SpeedMultiplier = 1.0f;

    private readonly Color _cNight = new(0.05f, 0.08f, 0.2f);
    private readonly Color _cDawn  = new(0.85f, 0.45f, 0.2f);
    private readonly Color _cNoon  = new(1.0f, 0.96f, 0.88f);
    private readonly Color _cDusk  = new(0.75f, 0.3f, 0.15f);

    public override void _Process(double delta)
    {
        CurrentTime = (CurrentTime + (float)delta / DaySeconds * SpeedMultiplier) % 1.0f;

        Vector3 rot = RotationDegrees;
        rot.X = (CurrentTime * 360.0f) - 90.0f;
        RotationDegrees = rot;

        float uDawn = Mathf.Max(0.0f, 1.0f - Mathf.Abs(CurrentTime - 0.25f) / 0.14f);
        float uNoon = Mathf.Max(0.0f, 1.0f - Mathf.Abs(CurrentTime - 0.50f) / 0.16f);
        float uDusk = Mathf.Max(0.0f, 1.0f - Mathf.Abs(CurrentTime - 0.75f) / 0.14f);
        float uNight = Mathf.Max(0.0f, 1.0f - (CurrentTime < 0.5f ? CurrentTime : 1.0f - CurrentTime) / 0.22f);
        float totalU = uNight + uDawn + uNoon + uDusk;

        LightColor = (_cNight * uNight + _cDawn * uDawn + _cNoon * uNoon + _cDusk * uDusk) / totalU;
        LightEnergy = Mathf.Clamp((uNoon * 1.2f + uDawn * 0.7f + uDusk * 0.7f), 0.05f, 1.2f);
    }
}",
                SceneTreeGuide = @"WorldEnvironment
 └─ DirectionalLight3D (Sun with day_night_fuzzy.gd)
     └─ WorldSky (ProceduralSkyMaterial: energy linked to light_energy)",
                SetupSteps = new List<string>
                {
                    "Attach script to main DirectionalLight3D.",
                    "Fuzzy membership sets eliminate harsh step transitions during dusk and dawn.",
                    "Interpolate light energy and sky shaders continuously via centroid defuzzification."
                }
            }
        }
    };
}