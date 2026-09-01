extends CharacterBody2D

signal defeated(at: Vector2)

var target: Node2D
var speed := 105.0
var health := 50
var contact_damage := 14
var attack_timer := 0.0

func _ready() -> void:
	collision_layer = 2
	collision_mask = 1 | 4
	add_to_group("enemies")
	var shape := CollisionShape2D.new()
	var circle := CircleShape2D.new()
	circle.radius = 18.0
	shape.shape = circle
	add_child(shape)
	queue_redraw()

func _physics_process(delta: float) -> void:
	attack_timer -= delta
	if not is_instance_valid(target):
		velocity = Vector2.ZERO
		return
	velocity = global_position.direction_to(target.global_position) * speed
	move_and_slide()
	for i in get_slide_collision_count():
		var body := get_slide_collision(i).get_collider()
		if body == target and attack_timer <= 0.0:
			target.take_damage(contact_damage)
			attack_timer = 0.65

func take_damage(amount: int) -> void:
	health -= amount
	if health <= 0:
		defeated.emit(global_position)
		queue_free()
	else:
		queue_redraw()

func _draw() -> void:
	draw_circle(Vector2.ZERO, 19.0, Color("ff477e"))
	draw_circle(Vector2.ZERO, 19.0, Color("ff9fbb"), false, 3.0)
	draw_circle(Vector2(-6, -3), 3.0, Color("240b36"))
	draw_circle(Vector2(6, -3), 3.0, Color("240b36"))
	draw_line(Vector2(-7, 7), Vector2(7, 7), Color("240b36"), 2.0)

