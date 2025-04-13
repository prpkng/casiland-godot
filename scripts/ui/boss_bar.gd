class_name BossBar extends Control

@onready var progress_bar := $ProgressBar

func set_max_health(value: int):
	progress_bar.max_value = value

func set_health(value: int):
	progress_bar.value = value
