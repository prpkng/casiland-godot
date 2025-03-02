@icon('res://editor/icons/node/icon_clear.png')
class_name DestroyOnHit extends Node
## Destroys the owner when the parent [Hitbox2D] hits something

func _ready() -> void:
	assert(get_parent() is HitBox2D, "'DestroyOnHit' must be children of a 'Hitbox2D'")
	
	(get_parent() as HitBox2D).action_applied.connect(destroy)

func destroy(_hurt_box: HurtBox2D) -> void:
	var hitbox = (get_parent() as HitBox2D)
	hitbox.ignore_collisions = true
	owner.queue_free()
