extends AnimatedSprite2D
class_name PlayerAnimations

func play_anim(name: StringName):
    if animation == name:
        return
    play(name)