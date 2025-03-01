@tool
extends BTAction
class_name BTTweenTask

var tween: Tween

# Called to generate a display name for the task (requires @tool).
func _generate_name() -> String:
	return "Tween Task"


# Called to initialize the task.
func _setup() -> void:
	pass


# Called when the task is entered.
func _enter() -> void:
	pass

# Called each time this task is ticked (aka executed).
func _tick(delta: float) -> Status:
	if tween == null or tween.is_running():
		return RUNNING
	return SUCCESS
