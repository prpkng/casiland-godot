@tool
extends BTAction

@export var boss_var: StringName
@export var value: bool

var boss: TheHandBoss

# Display a customized name (requires @tool).
func _generate_name() -> String:
	return "Stomp - Toogle Stick Shadow"

# Called once during initialization.
func _setup() -> void:
	boss = blackboard.get_var(boss_var)

func _enter() -> void:
	boss.stick_shadow.visible = value

func _tick(delta: float) -> Status:
	return SUCCESS
