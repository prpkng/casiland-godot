@tool
class_name RoomDesign
extends Control

@export var prop: PackedScene :
	set(v):
		prop = v
		_on_prop_switch()

var children: Node2D

func _on_prop_switch():
	if children != null: 
		children.free()
	children = prop.instantiate(PackedScene.GEN_EDIT_STATE_INSTANCE)
	add_child(children, false, Node.INTERNAL_MODE_BACK)

func _process(_delta: float) -> void:
	if children != null and Engine.is_editor_hint():
		var center := get_global_rect().get_center()
		children.global_position = center
