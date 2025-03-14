@tool
extends BTTweenTask
## PerformStomp

@export var duration: float = .35
@export var boss_var: StringName

var boss: TheHandBoss

# Display a customized name (requires @tool).
func _generate_name() -> String:
	return "Stomp - Pull Stomp"

# Called once during initialization.
func _setup() -> void:
	boss = blackboard.get_var(boss_var)

# Called each time this task is entered.
func _enter() -> void:
	tween = scene_root.get_tree().create_tween()
	
	tween.set_ease(Tween.EASE_OUT)
	tween.set_trans(Tween.TRANS_CUBIC)
	tween.tween_property(
		boss.pool_stick, 
		'position', 
		boss.pool_stick.position + Vector2.UP * 70,
		duration
	)
	

# Called each time this task is exited.
func _exit() -> void:
	pass
