extends Node2D

@export var movement_speed := 100

func _process(delta: float) -> void:
	position += transform.x * movement_speed * delta
