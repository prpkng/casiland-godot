extends AnimatedSprite2D
class_name PlayerAnimations

func play_anim(name: StringName):
	if animation == name:
		return
	play(name)
	
func _process(delta: float) -> void:
	flip_h = get_local_mouse_position().x < 0
