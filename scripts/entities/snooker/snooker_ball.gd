class_name SnookerBall extends RigidBody2D

const COLLISION_DAMAGE = 4

@onready var health = $Health

var boss: TheHandBoss

func _ready() -> void:
	body_entered.connect(_on_collision)
	health.died.connect(_on_death)

func _on_collision(body):
	$Health.damage(COLLISION_DAMAGE)
	print('collision')

func _on_death(_entity: Node):
	boss.destroy_ball(self)

func _process(_delta: float) -> void:
	pass
