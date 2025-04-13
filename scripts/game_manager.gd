class_name GameManager extends Node

const BOSS_BAR = preload('res://nodes/bosses/boss_bar.tscn')

var current_root: SubViewport
var player: Node2D
var ui: CanvasLayer
var ignoring_input: bool = false
var is_using_gamepad := false
var is_mobile := false
var player_aim_input: Vector2

func _ready() -> void:
	C.add_command('reset', reset, [], 0, 'Resets the current scene')

func _input(event: InputEvent) -> void:
	if event is InputEventKey or event is InputEventMouse:
		is_using_gamepad = false
	elif event is InputEventJoypadButton or event is InputEventJoypadMotion:
		is_using_gamepad = true
	elif is_mobile:
		is_using_gamepad = true

func create_boss_bar(max_health: int) -> Control:
	var boss_bar = BOSS_BAR.instantiate()
	ui.add_child(boss_bar)
	boss_bar.set_max_health(max_health)
	return boss_bar

func reset():
	get_tree().reload_current_scene()

func _process(delta: float) -> void:
	pass
