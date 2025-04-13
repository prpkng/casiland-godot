class_name CenteredProgress extends Control

var start_width: float
var start_x: float

@onready var bar_foreground: Control = $Foreground

func _ready() -> void:
	start_width = bar_foreground.size.x
	start_x = bar_foreground.position.x

func set_progress(progress: float):
	bar_foreground.size.x = lerp(start_width, 0.0, 1-progress)
	bar_foreground.position.x = start_x + lerp(0.0, start_width, 1-progress) / 2
