@tool
extends BTAction
## SetStickPos

@export var boss_var: StringName
@export var duration: BBFloat

var boss: TheHandBoss
var finished := false

# Display a customized name (requires @tool).
func _generate_name() -> String:
	return "Stomp - Follow Player"

# Called once during initialization.
func _setup() -> void:
	boss = blackboard.get_var(boss_var)


# Called each time this task is entered.
func _enter() -> void:
	boss.left_hand.play('Hold')
	boss.left_hand.reparent(boss.pool_stick)
	boss.left_hand.reset_physics_interpolation()
	boss.right_hand.play('Hold')
	boss.right_hand.reparent(boss.pool_stick)
	boss.right_hand.reset_physics_interpolation()
	
	var timer = scene_root.get_tree().create_timer(duration.get_value(scene_root, blackboard))
	await timer.timeout
	finished = true

# Called each time this task is exited.
func _exit() -> void:
	pass


# Called each time this task is ticked (aka executed).
func _tick(delta: float) -> Status:
	var stick = boss.pool_stick
	stick.global_position = \
		lerp(stick.global_position, GM.player.global_position + Vector2.UP * 20, delta * 4)
	stick.rotation = lerp_angle(stick.rotation, PI/2, delta * 4)
	var hand = boss.left_hand
	hand.position = lerp(hand.position, Vector2.LEFT * 28, delta * 4)
	hand.rotation = lerp_angle(hand.rotation, -PI/2, delta * 4)
	hand = boss.right_hand
	hand.position = lerp(hand.position, Vector2.LEFT * 76, delta * 4)
	hand.rotation = lerp_angle(hand.rotation, -PI/2, delta * 4)
	
	if !finished:
		return RUNNING
	return SUCCESS


# Strings returned from this method are displayed as warnings in the behavior tree editor (requires @tool).
func _get_configuration_warnings() -> PackedStringArray:
	var warnings := PackedStringArray()
	return warnings
