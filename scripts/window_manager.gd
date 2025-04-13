extends Node

func _input(event: InputEvent) -> void:
	if event.is_action_pressed('fullscreen_toggle'):
		var fullscreen = DisplayServer.window_get_mode() != \
			DisplayServer.WINDOW_MODE_WINDOWED
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_WINDOWED \
			if fullscreen else DisplayServer.WINDOW_MODE_FULLSCREEN)
		print(!fullscreen)
