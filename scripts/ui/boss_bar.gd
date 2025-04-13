class_name BossBar extends Control

var progress_bar: Control
var max_value: int

func _ready() -> void:
	var scn: PackedScene = load('res://nodes/bosses/the_hand/the_hand_bar_progress.tscn')
	progress_bar = scn.instantiate()
	add_child(progress_bar)

func set_max_health(value: int):
	max_value = value

func set_health(value: int):
	progress_bar.set_progress(value / float(max_value))
