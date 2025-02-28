extends SubViewportContainer

var start_size: Vector2

func _ready() -> void:
	get_tree().root.size_changed.connect(resized)
	start_size = size
	resized()


func resized():
	var root_size = get_tree().root.size as Vector2
	if root_size.x / 16 > root_size.y / 9:
		var aspect = root_size.x / root_size.y
		size = Vector2(start_size.y * aspect, start_size.y)
		position = -size/4
	else:
		var aspect = root_size.y / root_size.x
		size = Vector2(start_size.x, start_size.x * aspect)
		position = -size/4
