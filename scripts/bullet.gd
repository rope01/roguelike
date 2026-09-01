extends Area2D

var direction := Vector2.RIGHT
var speed := 760.0
var damage := 25
var lifetime := 1.4

func _ready() -> void:
	collision_layer = 4
	collision_mask = 2
	var shape := CollisionShape2D.new()
	var circle := CircleShape2D.new()
	circle.radius = 5.0
	shape.shape = circle
	add_child(shape)
	body_entered.connect(_on_body_entered)
	queue_redraw()

func _physics_process(delta: float) -> void:
	position += direction * speed * delta
	lifetime -= delta
	if lifetime <= 0.0:
		queue_free()

func _on_body_entered(body: Node) -> void:
	if body.has_method("take_damage"):
		body.take_damage(damage)
		queue_free()

func _draw() -> void:
	draw_circle(Vector2.ZERO, 6.0, Color("ffe66d"))
	draw_circle(Vector2.ZERO, 9.0, Color(1.0, 0.9, 0.35, 0.22))

