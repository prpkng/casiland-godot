extends RigidBody2D

const COLLISION_DAMAGE = 4
@onready var health = $Health

func _ready() -> void:
	body_entered.connect(_on_collision)

func _on_collision(body):
	$Health.damage(COLLISION_DAMAGE)
	print('collision')

func _process(_delta: float) -> void:
	pass
