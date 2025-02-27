extends Line2D

@export var offset: Vector2
@export var max_points_count := 20

var counter = 1

func _process(delta: float) -> void:
	global_position = Vector2.ZERO
	global_rotation = 0
	
		
	add_point(get_parent().global_position + offset)
	counter = 0
	
	while get_point_count() > max_points_count:
		remove_point(0)
