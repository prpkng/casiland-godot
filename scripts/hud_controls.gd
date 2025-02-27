extends Control


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if not OS.has_feature('mobile') and \
		not OS.has_feature('web_android') and \
		not OS.has_feature('web_ios'):
		queue_free()
	else:
		visible = true
