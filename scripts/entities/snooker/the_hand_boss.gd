extends Node2D
class_name TheHandBoss

const pool_ball = preload('res://nodes/pool/pool_ball.tscn')

@export var left_hand: AnimatedSprite2D
@export var right_hand: AnimatedSprite2D

@export var pool_table: Node2D

var hands_sine_active = false

const HANDS_SINE_FREQY = 1.25/1000
const HANDS_SINE_FREQX = HANDS_SINE_FREQY*2
const HANDS_SINE_AMPX = 6
const HANDS_SINE_AMPY = 16

func _ready() -> void:
	pass # Replace with function body.


func start_hands_sine():
	hands_sine_active = true

func stop_hands_sine():
	hands_sine_active = false

func perform_hands_sine(delta: float):
	# Left Hand
	var hand_origin = position + Vector2.RIGHT * 32 + Vector2.DOWN * 32
	hand_origin.y += sin(Time.get_ticks_msec() * HANDS_SINE_FREQY)  * HANDS_SINE_AMPY 
	hand_origin.x += sin(Time.get_ticks_msec() * HANDS_SINE_FREQX)  * HANDS_SINE_AMPX
	left_hand.global_position = lerp(left_hand.global_position, hand_origin, delta * 4)
	
	# Right Hand
	hand_origin = position + Vector2.LEFT * 32 + Vector2.DOWN * 32
	hand_origin.y += sin(Time.get_ticks_msec() * HANDS_SINE_FREQY)  * HANDS_SINE_AMPY 
	hand_origin.x -= sin(Time.get_ticks_msec() * HANDS_SINE_FREQX)  * HANDS_SINE_AMPX
	right_hand.global_position = lerp(right_hand.global_position, hand_origin, delta * 4)

func _process(delta: float) -> void:
	
	if hands_sine_active:
		perform_hands_sine(delta)


func spawn_ball(left: bool) -> void:
	var ball = pool_ball.instantiate()
	ball.get_node('Shadow').position.y += 32
	GM.current_root.add_child(ball)
	var hand = left_hand if left else right_hand
	hand.play('Carry')
	var tween = get_tree().create_tween()
	tween.set_ease(Tween.EASE_OUT)
	tween.set_trans(Tween.TRANS_CUBIC)
	ball.scale = Vector2.ZERO
	tween.tween_property(ball, 'scale', Vector2.ONE, .15)
	ball.physics_interpolation_mode = Node.PHYSICS_INTERPOLATION_MODE_OFF
	ball.global_position = hand.global_position + Vector2(0, 24)
	ball.process_mode = Node.PROCESS_MODE_DISABLED
	ball.reparent(hand)
	
