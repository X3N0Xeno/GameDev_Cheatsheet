using System.Collections.Generic;
using GameDevCheatsheet.Models;

namespace GameDevCheatsheet.Data.Categories;

public static class MovementCategory
{
    public static ComponentCategory GetCategory() => new()
    {
        Name = "PLAYER MOVEMENT",
        Icon = "🏃",
        IsExpanded = false,
        Items = new List<ComponentItem>
        {
            new()
            {
                Id = "platformer-2d",
                Title = "2D Platformer Controller",
                Category = "PLAYER MOVEMENT",
                SubRoute = "2D Platformer Controller",
                ScriptFilename = "player_platformer_2d.gd",
                GDScript = @"extends CharacterBody2D

@export var speed: float = 280.0
@export var jump_velocity: float = -420.0
@export var acceleration: float = 1400.0
@export var friction: float = 1200.0

@export var coyote_time: float = 0.12
@export var jump_buffer_time: float = 0.1

var coyote_timer: float = 0.0
var jump_buffer_timer: float = 0.0
var gravity: float = ProjectSettings.get_setting(""physics/2d/default_gravity"")

func _physics_process(delta: float) -> void:
    if not is_on_floor():
        velocity.y += gravity * delta
        coyote_timer -= delta
    else:
        coyote_timer = coyote_time

    if Input.is_action_just_pressed(""ui_accept""):
        jump_buffer_timer = jump_buffer_time
    else:
        jump_buffer_timer -= delta

    if jump_buffer_timer > 0.0 and coyote_timer > 0.0:
        velocity.y = jump_velocity
        jump_buffer_timer = 0.0
        coyote_timer = 0.0

    if Input.is_action_just_released(""ui_accept"") and velocity.y < 0.0:
        velocity.y *= 0.5

    var direction := Input.get_axis(""ui_left"", ""ui_right"")
    if direction != 0.0:
        velocity.x = move_toward(velocity.x, direction * speed, acceleration * delta)
    else:
        velocity.x = move_toward(velocity.x, 0.0, friction * delta)

    move_and_slide()",
                CSharpScript = @"using Godot;

public partial class PlayerPlatformer2D : CharacterBody2D
{
    [Export] public float Speed = 280.0f;
    [Export] public float JumpVelocity = -420.0f;
    [Export] public float Acceleration = 1400.0f;
    [Export] public float Friction = 1200.0f;
    [Export] public float CoyoteTime = 0.12f;
    [Export] public float JumpBufferTime = 0.1f;

    private float _coyoteTimer = 0.0f;
    private float _jumpBufferTimer = 0.0f;
    private float _gravity = (float)ProjectSettings.GetSetting(""physics/2d/default_gravity"");

    public override void _PhysicsProcess(double delta)
    {
        Vector2 vel = Velocity;
        float dt = (float)delta;

        if (!IsOnFloor()) { vel.Y += _gravity * dt; _coyoteTimer -= dt; }
        else { _coyoteTimer = CoyoteTime; }

        if (Input.IsActionJustPressed(""ui_accept"")) _jumpBufferTimer = JumpBufferTime;
        else _jumpBufferTimer -= dt;

        if (_jumpBufferTimer > 0.0f && _coyoteTimer > 0.0f)
        {
            vel.Y = JumpVelocity;
            _jumpBufferTimer = 0.0f;
            _coyoteTimer = 0.0f;
        }

        if (Input.IsActionJustReleased(""ui_accept"") && vel.Y < 0.0f) vel.Y *= 0.5f;

        float direction = Input.GetAxis(""ui_left"", ""ui_right"");
        vel.X = direction != 0.0f 
            ? Mathf.MoveToward(vel.X, direction * Speed, Acceleration * dt) 
            : Mathf.MoveToward(vel.X, 0.0f, Friction * dt);

        Velocity = vel;
        MoveAndSlide();
    }
}",
                SceneTreeGuide = @"Player (CharacterBody2D)
 ├─ CollisionShape2D (CapsuleShape2D: radius 8, height 28)
 ├─ AnimatedSprite2D
 └─ Camera2D (Position Smoothing Enabled: true)",
                SetupSteps = new List<string>
                {
                    "Attach script to CharacterBody2D root.",
                    "Add CapsuleShape2D and tune size to sprite boundaries.",
                    "Assign 'ui_left', 'ui_right', and 'ui_accept' in Project Settings > Input Map.",
                    "Enable position smoothing on Camera2D for cinematic lag."
                }
            },
            new()
            {
                Id = "topdown-2d",
                Title = "2D Top-Down Controller",
                Category = "PLAYER MOVEMENT",
                SubRoute = "2D Top-Down Controller",
                ScriptFilename = "topdown_controller_2d.gd",
                GDScript = @"extends CharacterBody2D

@export var max_speed: float = 240.0
@export var acceleration: float = 1600.0
@export var friction: float = 1400.0

func _physics_process(delta: float) -> void:
    var input_vector := Input.get_vector(""ui_left"", ""ui_right"", ""ui_up"", ""ui_down"")

    if input_vector != Vector2.ZERO:
        velocity = velocity.move_toward(input_vector * max_speed, acceleration * delta)
        rotation = lerp_angle(rotation, input_vector.angle(), 14.0 * delta)
    else:
        velocity = velocity.move_toward(Vector2.ZERO, friction * delta)

    move_and_slide()",
                CSharpScript = @"using Godot;

public partial class TopDownController2D : CharacterBody2D
{
    [Export] public float MaxSpeed = 240.0f;
    [Export] public float Acceleration = 1600.0f;
    [Export] public float Friction = 1400.0f;

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        Vector2 input = Input.GetVector(""ui_left"", ""ui_right"", ""ui_up"", ""ui_down"");

        if (input != Vector2.Zero)
        {
            Velocity = Velocity.MoveToward(input * MaxSpeed, Acceleration * dt);
            Rotation = (float)Mathf.LerpAngle(Rotation, input.Angle(), 14.0f * dt);
        }
        else
        {
            Velocity = Velocity.MoveToward(Vector2.Zero, Friction * dt);
        }

        MoveAndSlide();
    }
}",
                SceneTreeGuide = @"PlayerTopDown (CharacterBody2D - MotionMode: FLOATING)
 ├─ CollisionShape2D (CircleShape2D)
 └─ Sprite2D (Rotated so forward points right)",
                SetupSteps = new List<string>
                {
                    "Set CharacterBody2D motion_mode = FLOATING (disables gravity/floor checks).",
                    "Input.get_vector() clamps diagonals to prevent speed-boosting.",
                    "Lerp rotation angle for smooth facing transitions."
                }
            },
            new()
            {
                Id = "fighting-2d",
                Title = "2D Fighting Game Controller",
                Category = "PLAYER MOVEMENT",
                SubRoute = "2D Fighting Game Controller",
                ScriptFilename = "footsies_fighter_2d.gd",
                GDScript = @"extends CharacterBody2D

# Footsies Core: Spacing, Priority Clashes & Whiff Punish System
enum State { IDLE, WALK, CROUCH, JUMP, ATTACK, HITSTUN }
enum AttackPhase { NONE, STARTUP, ACTIVE, RECOVERY }

var current_state: State = State.IDLE
var attack_phase: AttackPhase = AttackPhase.NONE

@export var walk_speed: float = 160.0
@export var poke_damage: int = 12
@export var heavy_damage: int = 25

# Frame Data (60 FPS standard)
var frame_timer: int = 0
var startup_f: int = 0
var active_f: int = 0
var recovery_f: int = 0
var hitstun_f: int = 0

@onready var hurtbox: CollisionShape2D = $Hurtbox
@onready var hitbox: Area2D = $Hitbox
@onready var hitbox_shape: CollisionShape2D = $Hitbox/CollisionShape2D

signal clash_occurred()
signal counter_hit_confirmed(damage: int)

func _physics_process(delta: float) -> void:
    match current_state:
        State.IDLE, State.WALK:
            _handle_movement()
            if Input.is_action_just_pressed(""attack_light""):
                start_attack(4, 3, 8, poke_damage)  # Light Poke
            elif Input.is_action_just_pressed(""attack_heavy""):
                start_attack(9, 4, 18, heavy_damage) # Heavy Sweep

        State.ATTACK:
            velocity.x = move_toward(velocity.x, 0.0, 800.0 * delta)
            _advance_attack_frames()

        State.HITSTUN:
            velocity.x = move_toward(velocity.x, 0.0, 600.0 * delta)
            hitstun_f -= 1
            if hitstun_f <= 0:
                current_state = State.IDLE

    move_and_slide()

func _handle_movement() -> void:
    var dir := Input.get_axis(""ui_left"", ""ui_right"")
    if dir != 0.0:
        velocity.x = dir * walk_speed
        current_state = State.WALK
    else:
        velocity.x = 0.0
        current_state = State.IDLE

func start_attack(startup: int, active: int, recovery: int, dmg: int) -> void:
    current_state = State.ATTACK
    attack_phase = AttackPhase.STARTUP
    startup_f = startup
    active_f = active
    recovery_f = recovery
    frame_timer = 0
    hitbox.set_meta(""damage"", dmg)

func _advance_attack_frames() -> void:
    frame_timer += 1
    if frame_timer <= startup_f:
        attack_phase = AttackPhase.STARTUP
        hitbox_shape.disabled = true
    elif frame_timer <= startup_f + active_f:
        attack_phase = AttackPhase.ACTIVE
        hitbox_shape.disabled = false
    elif frame_timer <= startup_f + active_f + recovery_f:
        attack_phase = AttackPhase.RECOVERY
        hitbox_shape.disabled = true
    else:
        hitbox_shape.disabled = true
        attack_phase = AttackPhase.NONE
        current_state = State.IDLE

func take_hit(damage: int, hitstun: int, is_counter: bool) -> void:
    var final_dmg = int(damage * 1.5) if is_counter else damage
    hitstun_f = hitstun + (4 if is_counter else 0)
    current_state = State.HITSTUN
    hitbox_shape.disabled = true
    attack_phase = AttackPhase.NONE
    if is_counter:
        counter_hit_confirmed.emit(final_dmg)",
                CSharpScript = @"using Godot;

public partial class FootsiesFighter2D : CharacterBody2D
{
    public enum State { Idle, Walk, Crouch, Jump, Attack, Hitstun }
    public enum AttackPhase { None, Startup, Active, Recovery }

    public State CurrentState = State.Idle;
    public AttackPhase CurrentPhase = AttackPhase.None;

    [Export] public float WalkSpeed = 160.0f;
    [Export] public int PokeDamage = 12;
    [Export] public int HeavyDamage = 25;

    private int _frameTimer = 0;
    private int _startupF = 0;
    private int _activeF = 0;
    private int _recoveryF = 0;
    private int _hitstunF = 0;

    private CollisionShape2D _hurtbox;
    private Area2D _hitbox;
    private CollisionShape2D _hitboxShape;

    [Signal] public delegate void ClashOccurredEventHandler();
    [Signal] public delegate void CounterHitConfirmedEventHandler(int damage);

    public override void _Ready()
    {
        _hurtbox = GetNode<CollisionShape2D>(""Hurtbox"");
        _hitbox = GetNode<Area2D>(""Hitbox"");
        _hitboxShape = GetNode<CollisionShape2D>(""Hitbox/CollisionShape2D"");
        _hitboxShape.Disabled = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        switch (CurrentState)
        {
            case State.Idle:
            case State.Walk:
                HandleMovement();
                if (Input.IsActionJustPressed(""attack_light""))
                    StartAttack(4, 3, 8, PokeDamage);
                else if (Input.IsActionJustPressed(""attack_heavy""))
                    StartAttack(9, 4, 18, HeavyDamage);
                break;

            case State.Attack:
                Velocity = new Vector2(Mathf.MoveToward(Velocity.X, 0, 800 * dt), Velocity.Y);
                AdvanceAttackFrames();
                break;

            case State.Hitstun:
                Velocity = new Vector2(Mathf.MoveToward(Velocity.X, 0, 600 * dt), Velocity.Y);
                _hitstunF--;
                if (_hitstunF <= 0) CurrentState = State.Idle;
                break;
        }

        MoveAndSlide();
    }

    private void HandleMovement()
    {
        float dir = Input.GetAxis(""ui_left"", ""ui_right"");
        if (dir != 0)
        {
            Velocity = new Vector2(dir * WalkSpeed, Velocity.Y);
            CurrentState = State.Walk;
        }
        else
        {
            Velocity = new Vector2(0, Velocity.Y);
            CurrentState = State.Idle;
        }
    }

    public void StartAttack(int startup, int active, int recovery, int dmg)
    {
        CurrentState = State.Attack;
        CurrentPhase = AttackPhase.Startup;
        _startupF = startup;
        _activeF = active;
        _recoveryF = recovery;
        _frameTimer = 0;
        _hitbox.SetMeta(""damage"", dmg);
    }

    private void AdvanceAttackFrames()
    {
        _frameTimer++;
        if (_frameTimer <= _startupF)
        {
            CurrentPhase = AttackPhase.Startup;
            _hitboxShape.Disabled = true;
        }
        else if (_frameTimer <= _startupF + _activeF)
        {
            CurrentPhase = AttackPhase.Active;
            _hitboxShape.Disabled = false;
        }
        else if (_frameTimer <= _startupF + _activeF + _recoveryF)
        {
            CurrentPhase = AttackPhase.Recovery;
            _hitboxShape.Disabled = true;
        }
        else
        {
            _hitboxShape.Disabled = true;
            CurrentPhase = AttackPhase.None;
            CurrentState = State.Idle;
        }
    }

    public void TakeHit(int damage, int hitstun, bool isCounter)
    {
        int finalDmg = isCounter ? (int)(damage * 1.5f) : damage;
        _hitstunF = hitstun + (isCounter ? 4 : 0);
        CurrentState = State.Hitstun;
        _hitboxShape.Disabled = true;
        CurrentPhase = AttackPhase.None;
        if (isCounter) EmitSignal(SignalName.CounterHitConfirmed, finalDmg);
    }
}",
                SceneTreeGuide = @"Fighter (CharacterBody2D - CollisionLayer: 1)
 ├─ Hurtbox (CollisionShape2D: Capsule - Layer: 3)
 ├─ Hitbox (Area2D - Layer: 2, Mask: 3)
 │   └─ CollisionShape2D (Disabled: true, enabled only during active frames)
 └─ Pushbox (CollisionShape2D - Layer: 1, prevents walking through opponents)",
                SetupSteps = new List<string>
                {
                    "Configure project collision layers: 1 = Pushbox, 2 = Hitbox, 3 = Hurtbox.",
                    "Advance attack startup, active, and recovery phases frame-by-frame on a 60 FPS tick.",
                    "Set hitbox shape disabled = false only during AttackPhase.ACTIVE.",
                    "If an attack connects during opponent recovery frames, trigger counter_hit_confirmed (1.5x damage + 4F hitstun)."
                }
            },
            new()
            {
                Id = "fps-3d",
                Title = "3D First-Person (FPS)",
                Category = "PLAYER MOVEMENT",
                SubRoute = "3D First-Person (FPS)",
                ScriptFilename = "fps_controller_3d.gd",
                GDScript = @"extends CharacterBody3D

# Complete FPS: WASD Movement, Recoil Bloom, Headbob & Accurate Raycast
@export var walk_speed: float = 5.0
@export var sprint_speed: float = 8.5
@export var jump_velocity: float = 4.5
@export var mouse_sensitivity: float = 0.002

@export var recoil_climb: float = 0.04
@export var recoil_recovery_speed: float = 8.0
@export var headbob_freq: float = 2.4
@export var headbob_amp: float = 0.08

var headbob_time: float = 0.0
var target_recoil: float = 0.0
var gravity: float = ProjectSettings.get_setting(""physics/3d/default_gravity"")

@onready var head: Node3D = $Head
@onready var camera: Camera3D = $Head/Camera3D
@onready var raycast: RayCast3D = $Head/Camera3D/RayCast3D
@onready var weapon_mesh: Node3D = $Head/Camera3D/Weapon

func _ready() -> void:
    Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
    raycast.target_position = Vector3(0, 0, -100)

func _unhandled_input(event: InputEvent) -> void:
    if event is InputEventMouseMotion:
        rotate_y(-event.relative.x * mouse_sensitivity)
        head.rotate_x(-event.relative.y * mouse_sensitivity)
        head.rotation.x = clamp(head.rotation.x, deg_to_rad(-89), deg_to_rad(89))

func _physics_process(delta: float) -> void:
    if not is_on_floor():
        velocity.y -= gravity * delta

    if Input.is_action_just_pressed(""ui_accept"") and is_on_floor():
        velocity.y = jump_velocity

    var speed = sprint_speed if Input.is_action_pressed(""sprint"") else walk_speed
    var input_dir := Input.get_vector(""move_left"", ""move_right"", ""move_forward"", ""move_back"")
    var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()

    if direction:
        velocity.x = direction.x * speed
        velocity.z = direction.z * speed
        headbob_time += delta * speed
        camera.transform.origin.y = sin(headbob_time * headbob_freq) * headbob_amp
    else:
        velocity.x = move_toward(velocity.x, 0.0, speed)
        velocity.z = move_toward(velocity.z, 0.0, speed)
        camera.transform.origin.y = move_toward(camera.transform.origin.y, 0.0, delta * 2.0)

    # Recoil recovery
    head.rotation.x = move_toward(head.rotation.x, head.rotation.x - target_recoil, recoil_recovery_speed * delta)
    target_recoil = move_toward(target_recoil, 0.0, recoil_recovery_speed * delta)

    if Input.is_action_just_pressed(""fire""):
        shoot()

    move_and_slide()

func shoot() -> void:
    target_recoil += recoil_climb
    head.rotation.x += recoil_climb
    raycast.force_raycast_update()
    if raycast.is_colliding():
        var collider = raycast.get_collider()
        if collider.has_method(""take_damage""):
            collider.take_damage(25)",
                CSharpScript = @"using Godot;

public partial class FpsController3D : CharacterBody3D
{
    [Export] public float WalkSpeed = 5.0f;
    [Export] public float SprintSpeed = 8.5f;
    [Export] public float JumpVelocity = 4.5f;
    [Export] public float MouseSensitivity = 0.002f;
    [Export] public float RecoilClimb = 0.04f;
    [Export] public float RecoilRecoverySpeed = 8.0f;

    private Node3D _head;
    private Camera3D _camera;
    private RayCast3D _rayCast;
    private float _headbobTime = 0.0f;
    private float _targetRecoil = 0.0f;
    private float _gravity = (float)ProjectSettings.GetSetting(""physics/3d/default_gravity"");

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _head = GetNode<Node3D>(""Head"");
        _camera = GetNode<Camera3D>(""Head/Camera3D"");
        _rayCast = GetNode<RayCast3D>(""Head/Camera3D/RayCast3D"");
        _rayCast.TargetPosition = new Vector3(0, 0, -100);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            RotateY(-motion.Relative.X * MouseSensitivity);
            _head.RotateX(-motion.Relative.Y * MouseSensitivity);
            Vector3 rot = _head.Rotation;
            rot.X = Mathf.Clamp(rot.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));
            _head.Rotation = rot;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        Vector3 vel = Velocity;

        if (!IsOnFloor()) vel.Y -= _gravity * dt;
        if (Input.IsActionJustPressed(""ui_accept"") && IsOnFloor()) vel.Y = JumpVelocity;

        float speed = Input.IsActionPressed(""sprint"") ? SprintSpeed : WalkSpeed;
        Vector2 input = Input.GetVector(""move_left"", ""move_right"", ""move_forward"", ""move_back"");
        Vector3 dir = (Transform.Basis * new Vector3(input.X, 0, input.Y)).Normalized();

        if (dir != Vector3.Zero)
        {
            vel.X = dir.X * speed;
            vel.Z = dir.Z * speed;
            _headbobTime += dt * speed;
            Vector3 camPos = _camera.Transform.Origin;
            camPos.Y = Mathf.Sin(_headbobTime * 2.4f) * 0.08f;
            _camera.Transform = new Transform3D(_camera.Transform.Basis, camPos);
        }
        else
        {
            vel.X = Mathf.MoveToward(vel.X, 0, speed);
            vel.Z = Mathf.MoveToward(vel.Z, 0, speed);
        }

        Vector3 headRot = _head.Rotation;
        headRot.X = Mathf.MoveToward(headRot.X, headRot.X - _targetRecoil, RecoilRecoverySpeed * dt);
        _targetRecoil = Mathf.MoveToward(_targetRecoil, 0.0f, RecoilRecoverySpeed * dt);
        _head.Rotation = headRot;

        if (Input.IsActionJustPressed(""fire"")) Shoot();

        Velocity = vel;
        MoveAndSlide();
    }

    public void Shoot()
    {
        _targetRecoil += RecoilClimb;
        Vector3 r = _head.Rotation;
        r.X += RecoilClimb;
        _head.Rotation = r;

        _rayCast.ForceRaycastUpdate();
        if (_rayCast.IsColliding())
        {
            var collider = _rayCast.GetCollider();
            if (collider is Node n && n.HasMethod(""TakeDamage""))
                n.Call(""TakeDamage"", 25);
        }
    }
}",
                SceneTreeGuide = @"PlayerFPS (CharacterBody3D)
 ├─ CollisionShape3D (CapsuleShape3D: radius 0.4, height 1.8)
 └─ Head (Node3D - Height: 1.6m)
     └─ Camera3D
         ├─ RayCast3D (TargetPosition: Z = -100)
         └─ WeaponViewmodel (Node3D)",
                SetupSteps = new List<string>
                {
                    "Attach script to CharacterBody3D and lock mouse via Input.mouse_mode = CAPTURED.",
                    "Position Camera3D inside Head Node3D at eye-level (1.6m).",
                    "Add RayCast3D as direct child of Camera3D pointing straight along -Z axis.",
                    "Simulate realistic recoil by applying vertical impulse to Head.rotation.x and recovering smoothly with move_toward()."
                }
            },
            new()
            {
                Id = "thirdperson-3d",
                Title = "3D Third-Person Controller",
                Category = "PLAYER MOVEMENT",
                SubRoute = "3D Third-Person Controller",
                ScriptFilename = "third_person_controller.gd",
                GDScript = @"extends CharacterBody3D

# Third-Person: SpringArm3D Wall Clipping & Over-Shoulder POV Switch
@export var move_speed: float = 6.0
@export var rotation_speed: float = 12.0
@export var default_arm_length: float = 3.5
@export var pov_arm_length: float = 1.2
@export var pov_offset: Vector3 = Vector3(0.5, 0.2, 0.0)

var is_pov_mode: bool = false
var gravity: float = ProjectSettings.get_setting(""physics/3d/default_gravity"")

@onready var cam_pivot: Node3D = $CamPivot
@onready var spring_arm: SpringArm3D = $CamPivot/SpringArm3D
@onready var camera: Camera3D = $CamPivot/SpringArm3D/Camera3D
@onready var visuals: Node3D = $Visuals

func _ready() -> void:
    spring_arm.spring_length = default_arm_length
    spring_arm.add_excluded_object(get_rid()) # Do not collide with player hurtbox

func _unhandled_input(event: InputEvent) -> void:
    if event is InputEventMouseMotion:
        cam_pivot.rotate_y(-event.relative.x * 0.003)
        spring_arm.rotate_x(-event.relative.y * 0.003)
        spring_arm.rotation.x = clamp(spring_arm.rotation.x, deg_to_rad(-60), deg_to_rad(45))
        
    if event.is_action_just_pressed(""toggle_pov""):
        toggle_pov_camera()

func _physics_process(delta: float) -> void:
    if not is_on_floor():
        velocity.y -= gravity * delta

    var input_dir := Input.get_vector(""ui_left"", ""ui_right"", ""ui_up"", ""ui_down"")
    var cam_basis := cam_pivot.global_transform.basis
    var fwd := -cam_basis.z
    var right := cam_basis.x
    fwd.y = 0.0; right.y = 0.0
    var move_dir := (fwd * -input_dir.y + right * input_dir.x).normalized()

    if move_dir != Vector3.ZERO:
        velocity.x = move_dir.x * move_speed
        velocity.z = move_dir.z * move_speed
        
        # Face travel direction unless locked in over-the-shoulder POV mode
        if not is_pov_mode:
            var target_rot = atan2(-move_dir.x, -move_dir.z)
            visuals.rotation.y = lerp_angle(visuals.rotation.y, target_rot, rotation_speed * delta)
        else:
            visuals.rotation.y = cam_pivot.rotation.y
    else:
        velocity.x = move_toward(velocity.x, 0.0, move_speed)
        velocity.z = move_toward(velocity.z, 0.0, move_speed)

    move_and_slide()

func toggle_pov_camera() -> void:
    is_pov_mode = !is_pov_mode
    var tween = create_tween().set_parallel(true).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
    if is_pov_mode:
        tween.tween_property(spring_arm, ""spring_length"", pov_arm_length, 0.25)
        tween.tween_property(spring_arm, ""position"", pov_offset, 0.25)
    else:
        tween.tween_property(spring_arm, ""spring_length"", default_arm_length, 0.25)
        tween.tween_property(spring_arm, ""position"", Vector3.ZERO, 0.25)",
                CSharpScript = @"using Godot;

public partial class ThirdPersonController : CharacterBody3D
{
    [Export] public float MoveSpeed = 6.0f;
    [Export] public float RotationSpeed = 12.0f;
    [Export] public float DefaultArmLength = 3.5f;
    [Export] public float PovArmLength = 1.2f;
    [Export] public Vector3 PovOffset = new Vector3(0.5f, 0.2f, 0.0f);

    public bool IsPovMode = false;
    private Node3D _camPivot;
    private SpringArm3D _springArm;
    private Camera3D _camera;
    private Node3D _visuals;
    private float _gravity = (float)ProjectSettings.GetSetting(""physics/3d/default_gravity"");

    public override void _Ready()
    {
        _camPivot = GetNode<Node3D>(""CamPivot"");
        _springArm = GetNode<SpringArm3D>(""CamPivot/SpringArm3D"");
        _camera = GetNode<Camera3D>(""CamPivot/SpringArm3D/Camera3D"");
        _visuals = GetNode<Node3D>(""Visuals"");

        _springArm.SpringLength = DefaultArmLength;
        _springArm.AddExcludedObject(GetRid());
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            _camPivot.RotateY(-motion.Relative.X * 0.003f);
            _springArm.RotateX(-motion.Relative.Y * 0.003f);
            Vector3 rot = _springArm.Rotation;
            rot.X = Mathf.Clamp(rot.X, Mathf.DegToRad(-60), Mathf.DegToRad(45));
            _springArm.Rotation = rot;
        }

        if (@event.IsActionJustPressed(""toggle_pov"")) TogglePovCamera();
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        Vector3 vel = Velocity;
        if (!IsOnFloor()) vel.Y -= _gravity * dt;

        Vector2 input = Input.GetVector(""ui_left"", ""ui_right"", ""ui_up"", ""ui_down"");
        Basis camBasis = _camPivot.GlobalTransform.Basis;
        Vector3 fwd = new Vector3(-camBasis.Z.X, 0, -camBasis.Z.Z).Normalized();
        Vector3 right = new Vector3(camBasis.X.X, 0, camBasis.X.Z).Normalized();
        Vector3 moveDir = (fwd * -input.Y + right * input.X).Normalized();

        if (moveDir != Vector3.Zero)
        {
            vel.X = moveDir.X * MoveSpeed;
            vel.Z = moveDir.Z * MoveSpeed;

            if (!IsPovMode)
            {
                float targetRot = Mathf.Atan2(-moveDir.X, -moveDir.Z);
                Vector3 rot = _visuals.Rotation;
                rot.Y = (float)Mathf.LerpAngle(rot.Y, targetRot, RotationSpeed * dt);
                _visuals.Rotation = rot;
            }
            else
            {
                Vector3 rot = _visuals.Rotation;
                rot.Y = _camPivot.Rotation.Y;
                _visuals.Rotation = rot;
            }
        }
        else
        {
            vel.X = Mathf.MoveToward(vel.X, 0, MoveSpeed);
            vel.Z = Mathf.MoveToward(vel.Z, 0, MoveSpeed);
        }

        Velocity = vel;
        MoveAndSlide();
    }

    public void TogglePovCamera()
    {
        IsPovMode = !IsPovMode;
        var tween = CreateTween().SetParallel(true).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        if (IsPovMode)
        {
            tween.TweenProperty(_springArm, ""spring_length"", PovArmLength, 0.25);
            tween.TweenProperty(_springArm, ""position"", PovOffset, 0.25);
        }
        else
        {
            tween.TweenProperty(_springArm, ""spring_length"", DefaultArmLength, 0.25);
            tween.TweenProperty(_springArm, ""position"", Vector3.Zero, 0.25);
        }
    }
}",
                SceneTreeGuide = @"Player3D (CharacterBody3D)
 ├─ CollisionShape3D (CapsuleShape3D)
 ├─ Visuals (Node3D - Character model & armature)
 └─ CamPivot (Node3D - Handles Yaw)
     └─ SpringArm3D (Handles Pitch, Margin: 0.2m, Mask: World/Walls)
         └─ Camera3D (Child of SpringArm)",
                SetupSteps = new List<string>
                {
                    "Attach script to CharacterBody3D.",
                    "Use SpringArm3D with margin = 0.2m to prevent camera geometry clipping.",
                    "Always exclude player RID via spring_arm.add_excluded_object(get_rid()).",
                    "Smoothly tween SpringArm3D.spring_length and position offset when toggling between third-person orbit and over-the-shoulder POV."
                }
            }
        }
    };
}