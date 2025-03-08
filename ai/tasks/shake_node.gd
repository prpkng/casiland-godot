@tool
extends BTAction

@export var duration: BBFloat
@export var node: BBNode
@export var preset: ShakerPreset2D

@export var wait_for_completion: bool

var can_proceed := true

func _generate_name() -> String:
	return "Shake Node"

func _enter() -> void:
	var target = node.get_value(scene_root, blackboard)
	var d = duration.get_value(scene_root, blackboard)
	Shaker.shake_by_preset(preset, target, d, 1.0, 1.0, 0.0001, 0.0001)
	can_proceed = true
	if wait_for_completion:
		can_proceed = false
		await scene_root.get_tree().create_timer(d).timeout
		can_proceed = true

func _tick(delta: float) -> Status:
	if can_proceed:
		return SUCCESS
	return RUNNING
