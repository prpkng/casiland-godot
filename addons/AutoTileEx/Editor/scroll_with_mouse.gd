@tool
extends ScrollContainer


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


func _gui_input(event: InputEvent) -> void:
	if (event is InputEventMouseMotion):
		var motion = -event.relative 
		if !Input.is_mouse_button_pressed(MOUSE_BUTTON_MIDDLE): return
		
		scroll_horizontal += motion.x
		scroll_vertical += motion.y
