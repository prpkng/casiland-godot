extends VirtualJoystick

@export var roll_button: TouchScreenButton = null

func _input(event: InputEvent) -> void:
	if roll_button != null and !is_pressed and roll_button.is_pressed():
		return
	super(event)
