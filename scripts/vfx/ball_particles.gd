extends Node2D

func _ready() -> void:
    var tween := create_tween()
    var children = get_children() as Array[Sprite2D]

    tween.set_parallel(true)
    tween.set_ease(Tween.EASE_OUT)
    tween.set_trans(Tween.TRANS_CUBIC)

    for c in children:
        var dir = c.position.normalized()
        tween.tween_property(c, 'position', dir * 48, 1) 
        tween.tween_property(c, 'modulate:a', 0, 1) 
    
    await tween.finished
    if self:
        queue_free()