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
	return "Carry ball with hand"


# Called to initialize the task.
func _setup() -> void:
	boss = boss_node.get_value(scene_root, blackboard) as TheHandBoss


# Called when the task is entered.
func _enter() -> void:
	hand = boss.left_hand if is_left else boss.right_hand
	var dest_pos = boss.pool_table.global_position
	dest_pos.x += randf_range(-180, 180)
	dest_pos.y += randf_range(-80, 80)
	tween = scene_root.get_tree().create_tween()
	tween.set_ease(Tween.EASE_IN_OUT)
	tween.set_trans(Tween.TRANS_CUBIC)
	tween.tween_property(hand, 'global_position', dest_pos + Vector2.UP * 32, 1)


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
