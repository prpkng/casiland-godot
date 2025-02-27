extends Node


var ignoring_input: bool = false
var current_root: SubViewport

func _ready() -> void:
	C.add_command('reset', reset, [], 0, 'Resets the current scene')
	

func reset():
	get_tree().reload_current_scene()

func _process(delta: float) -> void:
	pass
