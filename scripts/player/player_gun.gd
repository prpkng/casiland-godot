extends Node2D
class_name PlayerGun

@export var gun_sprite: AnimatedSprite2D

@export var muzzle_point: Node2D

var start_x = 0

func _ready() -> void:
	start_x = position.x

func _process(delta):
	var dir = (get_global_mouse_position() - global_position).normalized()
	position.x = -start_x if dir.x < 0 else start_x
	gun_sprite.flip_v = dir.x < 0
	var angle = atan2(dir.y, dir.x)
	rotation = angle
	
