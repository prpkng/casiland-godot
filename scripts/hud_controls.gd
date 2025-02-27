extends Control

const OVERRIDE_MOBILE := false

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if not OS.has_feature('mobile') and \
		not OS.has_feature('web_android') and \
		not OS.has_feature('web_ios') and !OVERRIDE_MOBILE:
		queue_free()
	else:
		GM.is_mobile = true
		visible = true
