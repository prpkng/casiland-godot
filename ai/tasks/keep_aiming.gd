@tool
extends BTAction

# Task parameters.
@export var boss_node_var: StringName
@export var delay: BBFloat

var boss: TheHandBoss

var finished: bool

# Called to generate a display name for the task (requires @tool).
func _generate_name() -> String:
	return "Keep aiming"


# Called to initialize the task.
func _setup() -> void:
	boss = blackboard.get_var(boss_node_var) as TheHandBoss

func _enter() -> void:
	var duration = delay.get_value(scene_root, blackboard)
	print(duration)
	finished = false
	await scene_root.get_tree().create_timer(duration).timeout
	finished = true

func _tick(delta: float) -> Status:
	if boss and finished:
		return SUCCESS
	if not boss:
		return RUNNING
	
	var dir = (GM.player.position - boss.selected_ball.position).normalized()
	var angle = dir.angle()
	boss.pool_stick.global_position = boss.selected_ball.global_position - dir * 26
	boss.pool_stick.global_rotation = angle
	
	boss.left_hand.global_position =  boss.selected_ball.global_position - dir * 144
	boss.left_hand.global_rotation = angle
	
	boss.right_hand.global_position =  boss.selected_ball.global_position - dir * 64
	boss.right_hand.global_rotation = angle-PI
	
	return RUNNING
