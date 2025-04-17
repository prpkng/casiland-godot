class_name SnookerBall extends RigidBody2D

const COLLISION_DAMAGE := 4
const DEATH_PARTICLES := preload('res://nodes/vfx/ball_particles.tscn')
const HITBOX_FORCE := 2.0

@onready var health = $Health
@onready var hurtbox = $HurtBox2D

@onready var ball_hit_sound = $BallHitSound

var boss: TheHandBoss

func _ready() -> void:
	body_entered.connect(_on_collision)
	health.died.connect(_on_death)
	hurtbox.area_entered.connect(_on_damaged)

func _on_collision(_body):
	$Health.damage(COLLISION_DAMAGE)
	ball_hit_sound.play_one_shot()

func _on_damaged(entity: Node2D) -> void:
	if entity == null: return
	var dir: Vector2 = entity.global_transform.x
	apply_impulse(dir * HITBOX_FORCE)

func _on_death(_entity: Node):
	boss.destroy_ball(self)
	FX.set_aberration(1.5, 1)
	Camera.shake_mid()

	var particles: Node = DEATH_PARTICLES.instantiate()
	GM.current_root.add_child(particles)
	particles.global_position = global_position
	particles.global_rotation = global_rotation
	
	

func _process(_delta: float) -> void:
	pass
