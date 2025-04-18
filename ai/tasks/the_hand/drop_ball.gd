@tool
extends BTAction

# Task parameters.
@export var boss_node: BBNode
@export var is_left: bool

var boss: TheHandBoss
var hand: AnimatedSprite2D
var tween: Tween

# Called to generate a display name for the task (requires @tool).
func _generate_name() -> String:
	return "Drop ball"


# Called to initialize the task.
func _setup() -> void:
	boss = boss_node.get_value(scene_root, blackboard) as TheHandBoss


# Called when the task is entered.
func _enter() -> void:
	hand = boss.left_hand if is_left else boss.right_hand
	var ball = hand.get_node('Ball') as Node2D
	var shadow = ball.get_node('Shadow')
	shadow.reparent(GM.current_root)
	ball.reparent(GM.current_root)
	var dest_pos = ball.global_position + Vector2.DOWN * 32
	hand.play('Idle')
	tween = scene_root.get_tree().create_tween()
	tween.set_ease(Tween.EASE_IN)
	tween.set_trans(Tween.TRANS_CUBIC)
	tween.tween_property(ball, 'global_position', dest_pos, .75)
	await tween.finished
	ball.physics_interpolation_mode = Node.PHYSICS_INTERPOLATION_MODE_ON
	ball.process_mode = Node.PROCESS_MODE_INHERIT
	ball.reset_physics_interpolation()
	shadow.reparent(ball, false)
	shadow.position = Vector2.ZERO

# Called when the task is exited.
func _exit() -> void:
	pass


# Called each time this task is ticked (aka executed).
func _tick(_delta: float) -> Status:
	if tween.is_running():
		return RUNNING
	return SUCCESS


# Strings returned from this method are displayed as warnings in the editor.
func _get_configuration_warnings() -> PackedStringArray:
	var warnings := PackedStringArray()
	return warnings
