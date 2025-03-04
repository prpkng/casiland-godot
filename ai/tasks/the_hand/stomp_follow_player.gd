@tool
extends BTAction
## SetStickPos

@export var boss_var: StringName
@export var duration: BBFloat

var boss: TheHandBoss
var finished := false

# Display a customized name (requires @tool).
func _generate_name() -> String:
	return "Stomp - Follow Player"

# Called once during initialization.
func _setup() -> void:
	boss = blackboard.get_var(boss_var)


# Called each time this task is entered.
func _enter() -> void:
	var timer = scene_root.get_tree().create_timer(duration.get_value(scene_root, blackboard))
	await timer.timeout
	finished = true

# Called each time this task is exited.
func _exit() -> void:
	pass


# Called each time this task is ticked (aka executed).
func _tick(delta: float) -> Status:
	boss.pool_stick.global_position = GM.player.global_position + Vector2.UP * 16
	boss.pool_stick.rotation_degrees = 90
	
	if !finished:
		return RUNNING
	return SUCCESS


# Strings returned from this method are displayed as warnings in the behavior tree editor (requires @tool).
func _get_configuration_warnings() -> PackedStringArray:
	var warnings := PackedStringArray()
	return warnings
