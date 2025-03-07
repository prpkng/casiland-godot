@tool
extends BTAction
## SetStickPos

@export var boss_var: StringName

var boss: TheHandBoss
var finished := false

# Display a customized name (requires @tool).
func _generate_name() -> String:
	return "Stomp - Warn Stomp"

# Called once during initialization.
func _setup() -> void:
	boss = blackboard.get_var(boss_var)


# Called each time this task is entered.
func _enter() -> void:
	finished = false
	boss.pool_stick.get_node("Shaker").play_shake()
	var timer = scene_root.get_tree().create_timer(0.5)
	await timer.timeout
	finished = true

# Called each time this task is exited.
func _exit() -> void:
	pass


# Called each time this task is ticked (aka executed).
func _tick(delta: float) -> Status:
	if !finished:
		return RUNNING
	return SUCCESS


# Strings returned from this method are displayed as warnings in the behavior tree editor (requires @tool).
func _get_configuration_warnings() -> PackedStringArray:
	var warnings := PackedStringArray()
	return warnings
