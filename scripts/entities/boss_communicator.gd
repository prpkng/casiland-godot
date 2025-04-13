class_name BossCommunicator extends Node

@export var health: Health

var boss_bar: BossBar

func _ready() -> void:
	health.damaged.connect(damaged)
	boss_bar = GM.create_boss_bar(health.max)


func heal(_1, _amount, _2, _3):
	health_changed(health.current)


func damaged(_1, _amount, _2, _3):
	health_changed(health.current)
	

func health_changed(to: int):
	boss_bar.set_health(to)	
