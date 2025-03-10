extends Line2D

@export var offset: Vector2
@export var max_points_count := 5

var counter = 1

func _physics_process(_delta: float) -> void:
	global_position = Vector2.ZERO
	global_rotation = 0
	
		
	add_point(get_parent().global_position + offset)
	counter = 0
	
	while get_point_count() > max_points_count:
		remove_point(0)
