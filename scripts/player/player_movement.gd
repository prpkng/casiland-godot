extends CharacterBody2D

@export var movement_speed := 500
@export var acceleration := 0.75
@export var deceleration := 0.9
@export var roll_speed := 550
@export var roll_duration := 0.3

@export var player_sprite: PlayerAnimations
@export var player_gun: PlayerGun

@onready var hurt_box: HurtBox2D = $HurtBox2D

var roll_dir: Vector2
var move_input: Vector2
var is_rolling: bool

func _ready() -> void:
	GM.player = self

func _physics_process(_delta):
	if is_rolling:
		velocity = roll_dir * roll_speed
	else:
		var target_spd = move_input.normalized() * movement_speed
		var factor = deceleration if is_zero_approx(target_spd.length_squared()) else acceleration
		velocity = lerp(velocity, target_spd, factor);
		if target_spd.length_squared() > 0:
			player_sprite.play_anim('Run')
		else:
			player_sprite.play_anim('Idle')

	move_and_slide()

func roll():
	if is_rolling or move_input.length_squared() <= .05: return
	player_sprite.play_anim('Roll')
	
	is_rolling = true
	roll_dir = move_input.normalized()
	
	
	player_gun.visible = false
	GM.player_aim_input.x = roll_dir.x
	
	hurt_box.monitoring = false
	hurt_box.monitorable = false
	
	await get_tree().create_timer(roll_duration).timeout
	if !self: return
	
	hurt_box.monitoring = true
	hurt_box.monitorable = true
	
	is_rolling = false
	player_gun.visible = true
	
	

func _process(_delta):
	if GM.ignoring_input:
		move_input = Vector2.ZERO
		return
	move_input.x = Input.get_axis('move_left', 'move_right')
	move_input.y = Input.get_axis('move_up', 'move_down');

func _input(event: InputEvent) -> void:
	if event.is_action_pressed('roll'):
		roll()
