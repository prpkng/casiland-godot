extends AnimatedSprite2D

@export var ball_rb: RigidBody2D

const SPEED_MULTI = .05
const SPEED_POW = .75

func _ready() -> void:
	pass # Replace with function body.


func _physics_process(_delta: float) -> void:
	var dir = ball_rb.linear_velocity.normalized()
	var angle = atan2(dir.y, dir.x)
	#rotation = lerp_angle(rotation, angle, 0.5)
	rotation = angle
	#rotation_degrees = deg
	
	speed_scale = (ball_rb.linear_velocity.length() * SPEED_MULTI) ** SPEED_POW
