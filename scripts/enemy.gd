extends CharacterBody2D

signal defeated(at: Vector2)

var target: Node2D
var speed := 105.0
var health := 50
var contact_damage := 14
var attack_timer := 0.0
var body_color := Color("ef6a94")
var kind := 0

func configure(color: Color, speed_multiplier: float, hit_points: int, planet_kind: int) -> void:
	body_color = color
	speed *= speed_multiplier
	health = hit_points
	kind = planet_kind

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
	if kind == 0:
		draw_circle(Vector2.ZERO, 19.0, body_color)
		for i in 5:
			draw_circle(Vector2.from_angle(float(i) * TAU / 5.0) * 15.0, 7.0, body_color.lightened(0.12))
	elif kind == 1:
		draw_circle(Vector2.ZERO, 19.0, body_color)
		draw_circle(Vector2.ZERO, 13.0, Color("3a3340"))
		for i in 8:
			draw_line(Vector2.from_angle(float(i) * TAU / 8.0) * 18.0, Vector2.from_angle(float(i) * TAU / 8.0) * 24.0, body_color, 4.0)
	else:
		draw_polygon(PackedVector2Array([Vector2(-22, 0), Vector2(-5, -13), Vector2(4, -4), Vector2(22, -12), Vector2(14, 7), Vector2(-4, 11)]), PackedColorArray([body_color]))
	draw_circle(Vector2(-6, -2), 2.6, Color("1a1730"))
	draw_circle(Vector2(6, -2), 2.6, Color("1a1730"))
