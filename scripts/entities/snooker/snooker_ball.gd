extends RigidBody2D


func _ready() -> void:
	var dir = Vector2.from_angle(randf_range(0, 360))
	apply_impulse(dir * 300)
	pass # Replace with function body.


func _process(delta: float) -> void:
	pass
