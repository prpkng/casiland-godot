@icon('res://addons/fmod/icons/fmod_icon.svg')
@tool
extends BTAction
## PlayFmodEvent

@export var fmod_guid: BBString

var event_str: String

# Display a customized name (requires @tool).
func _generate_name() -> String:
	return "Play Fmod Event"


# Called once during initialization.
func _setup() -> void:
	event_str = fmod_guid.get_value(scene_root, blackboard)


# Called each time this task is entered.
func _enter() -> void:
	print(event_str)
	FmodServer.play_one_shot_using_guid_attached(event_str, scene_root)
	pass

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
