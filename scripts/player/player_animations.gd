extends AnimatedSprite2D
class_name PlayerAnimations

func play_anim(name: StringName):
	if animation == name:
		return
	play(name)
	
func _process(delta: float) -> void:
	scale.x = -1 if GM.player_aim_input.x < 0 else 1
