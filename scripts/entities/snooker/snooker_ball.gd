class_name SnookerBall extends RigidBody2D

const COLLISION_DAMAGE = 4
const DEATH_PARTICLES = preload('res://nodes/ball_particles.tscn')

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
	FX.set_aberration(1.5, 1)
	Camera.shake_mid()

	var particles = DEATH_PARTICLES.instantiate()
	GM.current_root.add_child(particles)
	particles.global_position = global_position
	particles.global_rotation = global_rotation
	
	

func _process(_delta: float) -> void:
	pass
