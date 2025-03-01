@tool
extends BTTweenTask

# Task parameters.
@export var boss_node_var: StringName

var boss: TheHandBoss
var ball: RigidBody2D

var start_lhand_pos: Vector2
var start_lhand_rot: float
var start_rhand_pos: Vector2
var start_rhand_rot: float
var start_stick_pos: Vector2
var start_stick_rot: float

# Called to generate a display name for the task (requires @tool).
func _generate_name() -> String:
	return "Tween hands to ball"


# Called to initialize the task.
func _setup() -> void:
	print(blackboard.get_var(boss_node_var))
	boss = blackboard.get_var(boss_node_var) as TheHandBoss
	

# Called when the task is entered.
func _enter() -> void:
	ball = boss.selected_ball
	
	start_stick_pos = boss.pool_stick.global_position
	start_stick_rot = boss.pool_stick.global_rotation
	
	start_lhand_pos = boss.left_hand.global_position
	start_lhand_rot = boss.left_hand.global_rotation
	start_rhand_pos = boss.right_hand.global_position
	start_rhand_rot = boss.right_hand.global_rotation
	
	await boss.get_tree().process_frame
	
	tween = scene_root.get_tree().create_tween()
	tween.set_ease(Tween.EASE_IN_OUT)
	tween.set_trans(Tween.TRANS_CUBIC)
	tween.tween_method(callback, -0.1, 1.0, 1.0)

func callback(t: float):
	var dir = (GM.player.position - boss.selected_ball.position).normalized()
	var angle = dir.angle()
	
	boss.pool_stick.global_position = lerp(start_stick_pos, ball.global_position - dir * 26, t)
	boss.pool_stick.global_rotation = lerp_angle(start_stick_rot, angle, t)
	
	boss.left_hand.play('Point')
	boss.left_hand.global_position = lerp(start_lhand_pos,  ball.global_position - dir * 144, t)
	boss.left_hand.global_rotation = lerp_angle(start_lhand_rot, angle, t)
	
	boss.right_hand.play('Pool')
	boss.right_hand.global_position = lerp(start_rhand_pos,  ball.global_position - dir * 64, t)
	boss.right_hand.global_rotation = lerp_angle(start_rhand_rot, angle-PI, t)

func _tick(delta: float) -> Status:
		
	return super(delta)
