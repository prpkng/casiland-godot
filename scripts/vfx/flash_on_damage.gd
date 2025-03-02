class_name FlashOnDamage extends Node
## Flashes the owning sprite when the [Health] component receives damage

## The target health component
@export var health: Health

## The target sprite
var spr: CanvasItem

## Setup signals
func _ready() -> void:
	health.damaged.connect(damaged)
	spr = get_parent() as CanvasItem
	spr.material = spr.material.duplicate()
	assert(spr != null, "ERROR: FlashOnDamage must be children of a CanvasItem")


## Called when the entity is damaged
func damaged(_entity: Node, _amount: int, _applied: int, _multiplier: float):
	if spr.material is ShaderMaterial:
		var mat = spr.material as ShaderMaterial
		mat.set_shader_parameter('flash', true)
		await get_tree().create_timer(.1, false).timeout
		mat.set_shader_parameter('flash', false)
