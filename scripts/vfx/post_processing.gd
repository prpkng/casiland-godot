class_name FX extends Control

@onready var flash_color_rect: ColorRect = $'Pass1/Flash Vignette'
@onready var chromatic_abberration_rect: ColorRect = $Pass2/Distortion

static var INST: FX

static var flash_color_tween: Tween
static var abberration_tween: Tween

func _ready() -> void:
	INST = self

static func flash_color(color: Color, duration: float, factor: float = 0.75):
	if flash_color_tween:
		flash_color_tween.stop()
	flash_color_tween = INST.get_tree().create_tween()
	flash_color_tween.set_ease(Tween.EASE_OUT)
	flash_color_tween.set_trans(Tween.TRANS_SINE)
	INST.flash_color_rect.color.a = factor
	flash_color_tween.tween_property(INST.flash_color_rect, 'color:a', 0, duration)
	INST.flash_color_rect.color = color
	INST.flash_color_rect.color.a = factor

static func set_aberration(force: float, duration: float):
	var rect = INST.chromatic_abberration_rect

	if abberration_tween:
		abberration_tween.stop()
	
	abberration_tween = INST.get_tree().create_tween()
	abberration_tween.set_ease(Tween.EASE_OUT)
	abberration_tween.set_trans(Tween.TRANS_SINE)
	abberration_tween.tween_method(
		func(value): rect.material.set_shader_parameter('spread', value / 100.0),
		force,
		0,
		duration
	)
