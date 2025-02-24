extends CharacterBody2D

@export var movement_speed := 500
@export var acceleration := 0.75
@export var deceleration := 0.9

var move_input: Vector2

func _physics_process(delta):
    var target_spd = move_input.normalized() * movement_speed
    var factor = deceleration if is_zero_approx(target_spd.length_squared()) else acceleration
    velocity = lerp(velocity, target_spd, factor);
    move_and_slide()
    

func _input(_event):
    move_input.x = Input.get_axis('move_left', 'move_right')
    move_input.y = Input.get_axis('move_up', 'move_down');