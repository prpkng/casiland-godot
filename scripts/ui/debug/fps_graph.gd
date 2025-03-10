extends "res://scripts/ui/debug/line_graph.gd"

@export var sample_per_sec = 30

@onready var fps_text: RichTextLabel = $"FPS Text"

var desired_fps: int

var last_time: int
var tick_counter: int

func toggle_active():
	visible = !visible
	set_process(visible)
	if visible:
		tick_counter = 0
		last_time = 0
		sample_fps()
		desired_fps = ceil(DisplayServer.screen_get_refresh_rate())

func _ready() -> void:
	sample_fps()
	
	desired_fps = ceil(DisplayServer.screen_get_refresh_rate())
	C.add_command('fps', toggle_active, [], 0, "Toggles the FPS counter on the bottom-right of the screen")
	C.add_hidden_command('show_fps', toggle_active, [], 0)

func _process(_delta: float) -> void:
	tick_counter += 1

func sample_fps() -> void:
	if !visible: return
	# Calculate FPS
	var dt = (Time.get_ticks_msec() - last_time) / 1000.0
	var fps = tick_counter / dt
	last_time = Time.get_ticks_msec()
	tick_counter = 0
	
	if fps != INF && fps != 0:
		add_sample(fps)
		var color = "#ffffffaa"
		if fps < 30:
			color = "#ff2222ee"
		elif fps < 50:
			color = '#ffff22aa'
		fps_text.text = " FPS [color=%s]%s" % [color, roundi(fps)]
	
	await get_tree().create_timer(1.0 / sample_per_sec).timeout
	if self:
		sample_fps()

func _draw() -> void:
	var y = clampf(inverse_lerp(sample_min, sample_max, desired_fps), 0, 1) * size.y
	draw_line(Vector2(1, size.y-y), Vector2(size.x-1, size.y-y), color/2, 1)
	
	super._draw()
