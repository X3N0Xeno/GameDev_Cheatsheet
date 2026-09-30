using System.Collections.Generic;
using GameDevCheatsheet.Models;

namespace GameDevCheatsheet.Data.Categories;

public static class CombatCategory
{
    public static ComponentCategory GetCategory() => new()
    {
        Name = "COMBAT & PROJECTILES",
        Icon = "⚔️",
        IsExpanded = false,
        Items = new List<ComponentItem>
        {
            new()
            {
                Id = "hitscan",
                Title = "Hitscan System",
                Category = "COMBAT & PROJECTILES",
                SubRoute = "Hitscan System",
                ScriptFilename = "hitscan_weapon.gd",
                GDScript = @"extends Node3D

@export var damage: float = 25.0
@export var max_range: float = 100.0
@export var impact_effect_scene: PackedScene

@onready var ray_cast: RayCast3D = $RayCast3D
@onready var muzzle_flash: GPUParticles3D = $MuzzleFlash

func _ready() -> void:
    ray_cast.target_position = Vector3(0, 0, -max_range)

func fire() -> void:
    muzzle_flash.restart()
    ray_cast.force_raycast_update()
    
    if ray_cast.is_colliding():
        var collider = ray_cast.get_collider()
        var hit_point = ray_cast.get_collision_point()
        var hit_normal = ray_cast.get_collision_normal()
        
        if collider.has_method(""take_damage""):
            collider.take_damage(damage)
            
        spawn_impact(hit_point, hit_normal)

func spawn_impact(pos: Vector3, normal: Vector3) -> void:
    if not impact_effect_scene: return
    var impact = impact_effect_scene.instantiate() as Node3D
    get_tree().root.add_child(impact)
    impact.global_position = pos
    if normal != Vector3.UP and normal != Vector3.DOWN:
        impact.look_at(pos + normal, Vector3.UP)",
                CSharpScript = @"using Godot;

public partial class HitscanWeapon : Node3D
{
    [Export] public float Damage = 25.0f;
    [Export] public float MaxRange = 100.0f;
    [Export] public PackedScene ImpactEffectScene;

    private RayCast3D _rayCast;
    private GpuParticles3D _muzzleFlash;

    public override void _Ready()
    {
        _rayCast = GetNode<RayCast3D>(""RayCast3D"");
        _muzzleFlash = GetNode<GpuParticles3D>(""MuzzleFlash"");
        _rayCast.TargetPosition = new Vector3(0, 0, -MaxRange);
    }

    public void Fire()
    {
        _muzzleFlash.Restart();
        _rayCast.ForceRaycastUpdate();

        if (_rayCast.IsColliding())
        {
            var collider = _rayCast.GetCollider();
            Vector3 hitPoint = _rayCast.GetCollisionPoint();
            Vector3 hitNormal = _rayCast.GetCollisionNormal();

            if (collider is Node target && target.HasMethod(""TakeDamage""))
            {
                target.Call(""TakeDamage"", Damage);
            }
            SpawnImpact(hitPoint, hitNormal);
        }
    }

    private void SpawnImpact(Vector3 pos, Vector3 normal)
    {
        if (ImpactEffectScene == null) return;
        var impact = ImpactEffectScene.Instantiate<Node3D>();
        GetTree().Root.AddChild(impact);
        impact.GlobalPosition = pos;
        if (normal != Vector3.Up && normal != Vector3.Down)
            impact.LookAt(pos + normal, Vector3.Up);
    }
}",
                SceneTreeGuide = @"Weapon (Node3D)
 ├─ RayCast3D (TargetPosition: Z = -100, CollisionMask: Environment/Enemies)
 ├─ MuzzleFlash (GPUParticles3D)
 └─ WeaponMesh (MeshInstance3D)",
                SetupSteps = new List<string>
                {
                    "Attach script to Weapon root node.",
                    "Set RayCast3D collision mask to scan enemies and geometry.",
                    "Call ray_cast.force_raycast_update() before reading collision state for zero-latency hits.",
                    "Spawn impact decal using collision normal to align orienting vectors."
                }
            },
            new()
            {
                Id = "projectile",
                Title = "RigidBody Projectile",
                Category = "COMBAT & PROJECTILES",
                SubRoute = "RigidBody Projectile",
                ScriptFilename = "projectile_spawner.gd",
                GDScript = @"extends Node3D

@export var projectile_scene: PackedScene
@export var launch_force: float = 35.0
@export var lifetime: float = 5.0

@onready var muzzle: Marker3D = $Muzzle

func spawn_projectile() -> void:
    if not projectile_scene: return
    var proj = projectile_scene.instantiate() as RigidBody3D
    get_tree().root.add_child(proj)
    proj.global_transform = muzzle.global_transform
    proj.apply_central_impulse(-muzzle.global_transform.basis.z * launch_force)
    
    get_tree().create_timer(lifetime).timeout.connect(proj.queue_free)",
                CSharpScript = @"using Godot;

public partial class ProjectileSpawner : Node3D
{
    [Export] public PackedScene ProjectileScene;
    [Export] public float LaunchForce = 35.0f;
    [Export] public float Lifetime = 5.0f;

    private Marker3D _muzzle;

    public override void _Ready()
    {
        _muzzle = GetNode<Marker3D>(""Muzzle"");
    }

    public void SpawnProjectile()
    {
        if (ProjectileScene == null) return;
        var proj = ProjectileScene.Instantiate<RigidBody3D>();
        GetTree().Root.AddChild(proj);
        proj.GlobalTransform = _muzzle.GlobalTransform;
        proj.ApplyCentralImpulse(-_muzzle.GlobalTransform.Basis.Z * LaunchForce);

        GetTree().CreateTimer(Lifetime).Timeout += proj.QueueFree;
    }
}",
                SceneTreeGuide = @"RocketLauncher (Node3D)
 └─ Muzzle (Marker3D - Position at barrel tip)
---
RocketProjectile.tscn (RigidBody3D - Continuous CD Enabled)
 ├─ CollisionShape3D (SphereShape3D)
 ├─ MeshInstance3D
 └─ TrailParticles (GPUParticles3D)",
                SetupSteps = new List<string>
                {
                    "Enable Continuous CD (CCD) on RigidBody3D to prevent tunneling through thin walls.",
                    "Set mass and gravity scale appropriately on projectile scene.",
                    "Apply launch vector along muzzle forward transform (-Basis.Z).",
                    "Add timer connected to queue_free() to prevent memory leaks."
                }
            },
            new()
            {
                Id = "melee-hitbox",
                Title = "Melee Hitbox Controller",
                Category = "COMBAT & PROJECTILES",
                SubRoute = "Melee Hitbox Controller",
                ScriptFilename = "melee_hitbox.gd",
                GDScript = @"extends Area2D

@export var damage: int = 25
@export var knockback_force: float = 300.0

var already_hit_targets: Array[Node2D] = []

func _ready() -> void:
    monitoring = false
    area_entered.connect(_on_area_entered)
    body_entered.connect(_on_body_entered)

func start_attack() -> void:
    already_hit_targets.clear()
    monitoring = true

func end_attack() -> void:
    monitoring = false

func _on_body_entered(body: Node2D) -> void:
    if body in already_hit_targets: return
    already_hit_targets.append(body)
    
    if body.has_method(""take_damage""):
        body.take_damage(damage)
    if body is CharacterBody2D:
        var dir = (body.global_position - global_position).normalized()
        body.velocity += dir * knockback_force",
                CSharpScript = @"using Godot;
using System.Collections.Generic;

public partial class MeleeHitbox : Area2D
{
    [Export] public int Damage = 25;
    [Export] public float KnockbackForce = 300.0f;

    private readonly List<Node2D> _hitTargets = new();

    public override void _Ready()
    {
        Monitoring = false;
        BodyEntered += OnBodyEntered;
    }

    public void StartAttack()
    {
        _hitTargets.Clear();
        Monitoring = true;
    }

    public void EndAttack()
    {
        Monitoring = false;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_hitTargets.Contains(body)) return;
        _hitTargets.Add(body);

        if (body.HasMethod(""TakeDamage""))
            body.Call(""TakeDamage"", Damage);

        if (body is CharacterBody2D character)
        {
            Vector2 dir = (body.GlobalPosition - GlobalPosition).Normalized();
            character.Velocity += dir * KnockbackForce;
        }
    }
}",
                SceneTreeGuide = @"SwordWeapon (Node2D)
 └─ Hitbox (Area2D - CollisionLayer: 4, CollisionMask: 2)
     ├─ CollisionPolygon2D (Swing arc shape)
     └─ AnimationPlayer (Toggles monitoring in method tracks)",
                SetupSteps = new List<string>
                {
                    "Isolate attack hitboxes from hurtboxes using dedicated collision layers.",
                    "Track already_hit_targets array so each entity only takes damage once per swing.",
                    "Enable and disable Area2D monitoring property inside AnimationPlayer call tracks.",
                    "Calculate knockback direction relative to player origin."
                }
            }
        }
    };
}