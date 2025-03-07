@tool
extends BTAction

enum Intensity {
	WEAK,
	MID,
	STRONG
}

@export var intensity: Intensity

func _generate_name() -> String:
	return "Shake Camera"


func _enter() -> void:
	match intensity:
		Intensity.WEAK:
			Camera.shake_weak()
		Intensity.MID:
			Camera.shake_mid()
		Intensity.STRONG:
			Camera.shake_strong()

func _tick(delta: float) -> Status:
	return SUCCESS
