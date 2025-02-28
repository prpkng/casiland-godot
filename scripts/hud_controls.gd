extends Control

const OVERRIDE_MOBILE := false

@export var left_stick: VirtualJoystick
@export var right_stick: VirtualJoystick

var was_holding_right_stick := false

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if not OS.has_feature('mobile') and \
		not OS.has_feature('web_android') and \
		not OS.has_feature('web_ios') and !OVERRIDE_MOBILE:
		queue_free()
	else:
		GM.is_mobile = true
		visible = true

func _process(delta: float) -> void:
	if right_stick.is_pressed and not was_holding_right_stick:
		was_holding_right_stick = true
		var event = InputEventAction.new()
		event.action = 'fire'
		event.pressed = true
		Input.parse_input_event(event)
	elif not right_stick.is_pressed and was_holding_right_stick:
		was_holding_right_stick = false
		var event = InputEventAction.new()
		event.action = 'fire'
		event.pressed = false
		Input.parse_input_event(event)

func _roll_pressed():
	var event = InputEventAction.new()
	event.action = 'roll'
	event.pressed = true
	Input.parse_input_event(event)
