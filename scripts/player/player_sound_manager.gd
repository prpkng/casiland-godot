extends Node

@export var health: Health

@onready var hurt_event: FmodEventEmitter2D = $HurtEventEmitter

func _ready() -> void:
	health.damaged.connect(on_damage)

func on_damage(_1, _2, _3, _4):
	hurt_event.play()
