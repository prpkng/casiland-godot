extends AnimatedSprite2D

@export var health: Health

var max_frames: int

func _ready() -> void:
	max_frames = sprite_frames.get_frame_count(animation)
	
	assert(health != null, "ERROR: Health must be set for 'HealthStages' node")
	health.damaged.connect(health_changed)

func health_changed(_entity, _amount, _applied, _multiplier):
	print('damaged: %s'% health.current)
	print(float(health.current) / float(health.max))

	frame = lerp(max_frames, 0, float(health.current) / float(health.max))
