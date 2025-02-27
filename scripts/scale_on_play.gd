extends Node2D
const sub_viewport = preload('res://nodes/sub_viewport.tscn')

func _ready() -> void:
	await get_parent().ready
	var viewport = sub_viewport.instantiate()
	get_parent().add_child(viewport)
	reparent(viewport.get_node('SubViewport'))
	GM.current_root = viewport.get_node('SubViewport')

func _process(delta: float) -> void:
	pass
