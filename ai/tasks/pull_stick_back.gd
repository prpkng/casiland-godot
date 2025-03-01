@tool
extends BTTweenTask

# Task parameters.
@export var boss_node_var: StringName

var boss: TheHandBoss

# Called to generate a display name for the task (requires @tool).
func _generate_name() -> String:
	return "Pull stick back"


# Called to initialize the task.
func _setup() -> void:
	boss = blackboard.get_var(boss_node_var) as TheHandBoss

func _enter() -> void:
	boss.pool_stick.reparent(boss.selected_ball)
	boss.left_hand.reparent(boss.selected_ball)
	boss.right_hand.reparent(boss.selected_ball)
	tween = scene_root.get_tree().create_tween()
	tween.set_ease(Tween.EASE_OUT)
	tween.set_trans(Tween.TRANS_CUBIC)
	tween.set_parallel(true)
	var dir = (boss.selected_ball.global_position - boss.pool_stick.global_position).normalized()
	tween.tween_property(boss.pool_stick, 'position', boss.pool_stick.position - dir * 24, .5)
	tween.tween_property(boss.left_hand, 'position', boss.left_hand.position - dir * 24, .5)
