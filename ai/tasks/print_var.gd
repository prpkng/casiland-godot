extends BTAction

@export var target_var: StringName

func _tick(_delta: float) -> Status:
	print(blackboard.get_var(target_var))
	return SUCCESS
