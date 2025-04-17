class_name Camera extends Camera2D

const SHAKE_STRONG = preload('res://resources/shakes/cam_shake_strong.tres')
const SHAKE_MID = preload('res://resources/shakes/cam_shake_mid.tres')
const SHAKE_WEAK = preload('res://resources/shakes/cam_shake_weak.tres')

static var INSTANCE: Camera2D

@export var target: Node2D
@export var target_override: Node2D

#@onready var shaker = $ShakerComponent2D

func _ready() -> void:
	INSTANCE = self
	if target == null:
		target = get_tree().get_first_node_in_group('player')


func _process(_delta: float) -> void:
	position = target.position if target_override == null else target_override.position
	
	# Update listener position
	FmodServer.set_listener_transform2d(0, transform)

static func shake_strong():
	Shaker.shake_by_preset(SHAKE_STRONG, INSTANCE, .45, 1, 1, 0.0001)
	
static func shake_mid():
	Shaker.shake_by_preset(SHAKE_MID, INSTANCE, .3, 1, 1, 0.0001)
	
static func shake_weak():
	Shaker.shake_by_preset(SHAKE_WEAK, INSTANCE, .3, 1, 1, 0.0001)
