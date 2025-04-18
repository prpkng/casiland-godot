@tool
extends BTAction
## SetFmodAggressive

@export var aggressive: bool

# Display a customized name (requires @tool).
func _generate_name() -> String:
	return "Set Fmod Aggressive"


# Called once during initialization.
func _setup() -> void:
	pass


# Called each time this task is entered.
func _enter() -> void:
	GM.music.set_aggressive(aggressive)


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
