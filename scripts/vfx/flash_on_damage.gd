class_name FlashOnDamage extends Node
## Flashes the owning sprite when the [Health] component receives damage

## The target hurtbox component
@export var hurtbox: HurtBox2D

## The target sprite
var spr: CanvasItem

## Setup signals
func _ready() -> void:
	hurtbox.area_entered.connect(damaged)
	spr = get_parent() as CanvasItem
	spr.material = spr.material.duplicate()
	assert(spr != null, "ERROR: FlashOnDamage must be children of a CanvasItem")


## Called when the entity is damaged
func damaged(_node: Node):
	if spr.material is ShaderMaterial:
		spr.set_instance_shader_parameter('flash', true)
		await get_tree().create_timer(.1, false).timeout
		spr.set_instance_shader_parameter('flash', false)
