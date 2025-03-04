@tool
extends BTTweenTask

# Task parameters.
@export var boss_node_var: StringName

var boss: TheHandBoss

# Called to generate a display name for the task (requires @tool).
func _generate_name() -> String:
	return "Push stick"


# Called to initialize the task.
func _setup() -> void:
	boss = blackboard.get_var(boss_node_var) as TheHandBoss

func _enter() -> void:
	tween = scene_root.get_tree().create_tween()
	tween.set_ease(Tween.EASE_IN)
	tween.set_trans(Tween.TRANS_LINEAR)
	tween.set_parallel(true)
	var dir = (boss.selected_ball.global_position - boss.pool_stick.global_position).normalized()
	tween.tween_property(boss.pool_stick, 'position', boss.pool_stick.position + dir * 25, .075)
	tween.tween_property(boss.left_hand, 'position', boss.left_hand.position + dir * 25, .075)

func _exit() -> void:
	if boss.selected_ball == null:
		return
	boss.push_ball((boss.selected_ball.global_position - boss.pool_stick.global_position).normalized())
	boss.pool_stick.reparent(boss)
	boss.pool_stick.reset_physics_interpolation()
	boss.left_hand.reparent(boss)
	boss.left_hand.reset_physics_interpolation()
	boss.right_hand.reparent(boss)
	boss.right_hand.reset_physics_interpolation()
	
