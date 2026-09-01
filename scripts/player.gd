extends CharacterBody2D

signal died
signal health_changed(value: int, maximum: int)

const BulletScene = preload("res://scripts/bullet.gd")

var speed := 310.0
var max_health := 100
var health := 100
var fire_delay := 0.22
var fire_timer := 0.0
var damage := 25
var invulnerability := 0.0

func _ready() -> void:
	collision_layer = 1
	collision_mask = 2
	var shape := CollisionShape2D.new()
	var circle := CircleShape2D.new()
	circle.radius = 17.0
	shape.shape = circle
	add_child(shape)
	queue_redraw()

func _physics_process(delta: float) -> void:
	var direction := Input.get_vector("move_left", "move_right", "move_up", "move_down")
	velocity = direction * speed
	move_and_slide()
	var screen := get_viewport_rect().size
	var from_center := position - screen / 2.0
	if from_center.length() > 245.0:
		position = screen / 2.0 + from_center.normalized() * 245.0
	look_at(get_global_mouse_position())
	fire_timer -= delta
	invulnerability -= delta
	if Input.is_action_pressed("shoot") and fire_timer <= 0.0:
		shoot()
		fire_timer = fire_delay
	queue_redraw()

func shoot() -> void:
	var bullet := BulletScene.new()
	bullet.global_position = global_position + Vector2.RIGHT.rotated(rotation) * 25.0
	bullet.direction = Vector2.RIGHT.rotated(rotation)
	bullet.damage = damage
	get_parent().add_child(bullet)

func take_damage(amount: int) -> void:
	if invulnerability > 0.0 or health <= 0:
		return
	health = maxi(0, health - amount)
	invulnerability = 0.45
	health_changed.emit(health, max_health)
	if health == 0:
		died.emit()
		queue_free()

func upgrade() -> void:
	damage += 5
	fire_delay = maxf(0.09, fire_delay - 0.015)
	health = mini(max_health, health + 15)
	health_changed.emit(health, max_health)

func _draw() -> void:
	var flash := invulnerability > 0.0 and int(invulnerability * 16.0) % 2 == 0
	var cloak := Color(1.0, 1.0, 1.0, 0.35) if flash else Color("394b73")
	# Плащ и голова грустного странствующего музыканта — вид сверху.
	draw_polygon(PackedVector2Array([Vector2(-18, -15), Vector2(15, -12), Vector2(19, 13), Vector2(-18, 16)]), PackedColorArray([cloak]))
	draw_circle(Vector2(-5, 0), 11.0, Color("e8c7a1"))
	draw_arc(Vector2(-7, 0), 11.5, -2.2, 2.2, 18, Color("25304a"), 5.0)
	# Маленькая лютня; её струны выпускают ноты.
	draw_circle(Vector2(12, 4), 9.0, Color("b9784b"))
	draw_line(Vector2(15, 1), Vector2(31, -8), Color("d8a36c"), 5.0)
	draw_line(Vector2(3, -7), Vector2(20, 11), Color("f3d39c"), 1.4)
	draw_circle(Vector2(12, 4), 3.0, Color("563629"))
