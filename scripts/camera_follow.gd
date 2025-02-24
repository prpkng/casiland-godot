extends Camera2D

@export var target: Node2D
@export var target_override: Node2D

func _ready() -> void:
	if target == null:
		target = get_tree().get_first_node_in_group('player')


func _process(_delta: float) -> void:
	position = target.position if target_override == null else target_override.position
