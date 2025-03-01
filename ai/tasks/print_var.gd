extends BTAction

@export var target: StringName

func _tick(delta: float) -> Status:
	print(blackboard.get_var(target))
	return SUCCESS
