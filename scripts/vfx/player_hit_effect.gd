extends Node

const OVERLAY_RED := Color.RED


func _ready() -> void:
	var health = get_parent() as Health
	
	health.damaged.connect(_damaged)


func _damaged(_entity: Node, _amount: int, _applied: int, _multiplier: float):
	Camera.shake_strong()
	FX.flash_color(OVERLAY_RED, 1)
	FX.set_aberration(2.5, 1.75)
