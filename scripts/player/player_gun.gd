@icon('res://editor/icons/node_2D/icon_sword.png')
extends Node2D
class_name PlayerGun

const player_bullet: PackedScene = preload('res://nodes/player_bullet.tscn')

@export var muzzle_point: Node2D
@export var fire_rate: float = 6
@export var bullet_damage: float = 10

@onready var gun_sprite: AnimatedSprite2D = $GunSprite

var start_x = 0
var is_firing := false
var fire_counter := 1.0
var recoil_tween: Tween

func _ready() -> void:
	start_x = position.x


func _process(delta):
	calculate_aim()
	
	fire_counter += delta * fire_rate
	if not is_firing: return
	if fire_counter > 1:
		fire()

func calculate_aim():
	var dir: Vector2
	if GM.is_using_gamepad:
		var vec = Input.get_vector('aim_left', 'aim_right', 'aim_up', 'aim_down')
		if vec.length_squared() <= 0: return
		dir = vec
	else:
		dir = (get_global_mouse_position() - get_parent().global_position).normalized()
	GM.player_aim_input = dir
	var angle = atan2(dir.y, dir.x)
	global_rotation = angle

func start_firing():
	is_firing = true
	if fire_counter < 1: return
	fire_counter = 0
	fire()

func stop_firing():
	is_firing = false

func fire():
	fire_counter = 0
	var bullet = player_bullet.instantiate()
	GM.current_root.add_child(bullet)
	bullet.global_position = muzzle_point.global_position
	bullet.set_process(true)
	bullet.visible = true
	(bullet.get_node('HitBox2D') as HitBox2D).amount = bullet_damage
	get_tree().create_timer(1).timeout.connect(bullet.queue_free)
	bullet.rotation = global_rotation
	
	# Recoil
	if recoil_tween:
		recoil_tween.stop()
	recoil_tween = create_tween()
	recoil_tween.set_ease(Tween.EASE_OUT)
	recoil_tween.set_trans(Tween.TRANS_CUBIC)
	
	var recoil_force = 2 + (bullet_damage / 10.0 - 1) * 1.5
	
	gun_sprite.position = -transform.x * recoil_force
	recoil_tween.tween_property(gun_sprite, 'position', Vector2.ZERO, 0.25)
	

func _unhandled_input(event: InputEvent) -> void:
	if GM.ignoring_input: return
	
	if event.is_action_pressed('fire'):
		start_firing()
	
	if event.is_action_released('fire'):
		stop_firing()
	
