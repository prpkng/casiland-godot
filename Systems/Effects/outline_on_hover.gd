extends Node2D

const ENABLED_PARAMETER = &"outline_enabled"
@export var interactable: Interactable


func _ready() -> void:
	interactable.BeginHover.connect(begin_hover)
	interactable.EndHover.connect(end_hover)


func begin_hover(_other: Area2D) -> void:
	set_instance_shader_parameter(ENABLED_PARAMETER, true)


func end_hover(_other: Area2D) -> void:
	set_instance_shader_parameter(ENABLED_PARAMETER, false)
