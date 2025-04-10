extends AnimatedSprite2D

const BALL_COLOR_COUNT := 8
const SPEED_MULTI := .05
const SPEED_POW := .75

const LINE_BALL_SPR: SpriteFrames = preload('res://graphics/spr/the_hand/line_ball.aseprite')
const FLAT_BALL_SPR: SpriteFrames = preload('res://graphics/spr/the_hand/flat_ball.aseprite')

static var used_colors: Array[int] = []

@export var ball_rb: RigidBody2D

var color_idx: int


func _ready() -> void:
	var possible_indexes := range(BALL_COLOR_COUNT * 2)
	var available_indexes: Array[int] = []
	for i in possible_indexes:
		if !used_colors.has(i):
			available_indexes.append(i)
	
	color_idx = available_indexes.pick_random()
	used_colors.append(color_idx)
	
	if color_idx > 7:
		color_idx -= 8
		sprite_frames = FLAT_BALL_SPR
	else:
		sprite_frames = LINE_BALL_SPR 
		if color_idx >= 15:
			sprite_frames = FLAT_BALL_SPR
	
	print(color_idx)
	
	set_instance_shader_parameter('pallete_index', color_idx)


func _physics_process(_delta: float) -> void:
	var dir: Vector2 = ball_rb.linear_velocity.normalized()
	var angle: float = atan2(dir.y, dir.x)
	#rotation = lerp_angle(rotation, angle, 0.5)
	rotation = angle
	#rotation_degrees = deg
	
	speed_scale = (ball_rb.linear_velocity.length() * SPEED_MULTI) ** SPEED_POW
