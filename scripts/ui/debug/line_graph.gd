@icon("res://editor/icons/control/icon_stat.png")
extends Panel

@export var sample_min = 5
@export var sample_max = 80
@export var sample_view_count = 50
@export var dynamically_resize: bool

@export var color: Color

var samples: Array[float]

var sample_width : float : 
	get():
		return size.x / float(sample_view_count)

func add_sample(sample: float):
	samples.push_front(sample)
	if samples.size() > sample_view_count:
		for i in range(sample_view_count/4):
			samples.remove_at(0)
	queue_redraw()
	
	if !dynamically_resize: return
	
	var highest_sample = samples.max()
	var lowest_sample = samples.min()
	
	sample_max = highest_sample + 20
	sample_min = lowest_sample - 20

func _draw() -> void:
	if samples.is_empty(): return
	
	var i = 0
	for sample in samples:
		var x = i+1
		var y = clampf(inverse_lerp(sample_min, sample_max, sample), 0, 1) * size.y
		draw_line(Vector2(x, size.y), Vector2(x, size.y-y), color, sample_width)
		i += sample_width
