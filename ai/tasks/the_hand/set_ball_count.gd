@tool
extends BTAction
## SetBallCount

@export var boss: BBNode
@export var ball_count_var: StringName

# Display a customized name (requires @tool).
func _generate_name() -> String:
	return "Set Ball Count"


# Called once during initialization.
func _setup() -> void:
	pass


# Called each time this task is entered.
func _enter() -> void:
	var boss = boss.get_value(scene_root, blackboard) as TheHandBoss
	blackboard.set_var(ball_count_var, boss.active_balls.size())


# Called each time this task is exited.
func _exit() -> void:
	pass


# Called each time this task is ticked (aka executed).
func _tick(delta: float) -> Status:
	return SUCCESS


# Strings returned from this method are displayed as warnings in the behavior tree editor (requires @tool).
func _get_configuration_warnings() -> PackedStringArray:
	var warnings := PackedStringArray()
	return warnings
