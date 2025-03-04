class_name FX extends Control

@onready var flash_color_rect: ColorRect = $Pass1/Flash

static var INST: FX

static var flash_color_tween: Tween

func _ready() -> void:
	INST = self

static func flash_color(color: Color, duration: float, factor: float = 0.75):
	if flash_color_tween:
		flash_color_tween.stop()
	flash_color_tween = INST.get_tree().create_tween()
	flash_color_tween.set_ease(Tween.EASE_OUT)
	flash_color_tween.set_trans(Tween.TRANS_SINE)
	flash_color_tween.tween_method(_set_color, factor, 0, duration)
	INST.flash_color_rect.color = color
	INST.flash_color_rect.color.a = factor

static func _set_color(fac: float):
	INST.flash_color_rect.color.a = fac
