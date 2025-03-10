extends "res://scripts/ui/debug/line_graph.gd"

@export var sample_per_sec = 2

@onready var mem_text: RichTextLabel = $"MEM Text"

var desired_mem: int

func toggle_active():
	visible = !visible
	set_process(visible)
	if visible:
		sample_mem()
		desired_mem = ceil(DisplayServer.screen_get_refresh_rate())

func _ready() -> void:
	sample_mem()
	
	desired_mem = ceil(DisplayServer.screen_get_refresh_rate())
	C.add_command('mem', toggle_active, [], 0, "Toggles the memory counter on the bottom-right of the screen")
	C.add_hidden_command('show_mem', toggle_active, [], 0)


func sample_mem() -> void:
	if !visible: return
	# Calculate FPS
	var mem = Performance.get_monitor(Performance.Monitor.MEMORY_STATIC) / 1024.0 / 1024.0
	
	if mem != INF && mem != 0:
		add_sample(mem)
		var clr = "#ffffffaa"
		if mem > 100:
			clr = '#ffff22aa'
		elif mem > 70:
			clr = "#ff2222ee"
		mem_text.text = " MEM [color=%s]%smb" % [clr, roundi(mem)]
	
	await get_tree().create_timer(1.0 / sample_per_sec).timeout
	if self:
		sample_mem()
