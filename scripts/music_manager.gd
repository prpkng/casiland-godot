class_name MusicManager extends FmodEventEmitter2D

func _enter_tree() -> void:
	GM.music = self

func _exit_tree() -> void:
	if GM.music == self:
		GM.music = null


func set_aggressive(aggressive: bool):
	set_parameter("AggressiveAct", 1 if aggressive else 0)
