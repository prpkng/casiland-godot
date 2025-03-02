@tool
extends BTAction

# Task parameters.
@export var boss_node: BBNode

var boss: TheHandBoss

# Called to generate a display name for the task (requires @tool).
func _generate_name() -> String:
	return "Select random ball"


# Called to initialize the task.
func _setup() -> void:
	boss = boss_node.get_value(scene_root, blackboard) as TheHandBoss


# Called when the task is entered.
func _enter() -> void:
	boss.pick_random_ball()

# Called when the task is exited.
func _exit() -> void:
	pass


# Called each time this task is ticked (aka executed).
func _tick(_delta: float) -> Status:
	return SUCCESS


# Strings returned from this method are displayed as warnings in the editor.
func _get_configuration_warnings() -> PackedStringArray:
	var warnings := PackedStringArray()
	return warnings
