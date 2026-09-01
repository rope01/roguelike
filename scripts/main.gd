extends Node2D

const PlayerScene = preload("res://scripts/player.gd")
const EnemyScene = preload("res://scripts/enemy.gd")
const HudScene = preload("res://scripts/hud.gd")

var player: CharacterBody2D
var hud: CanvasLayer
var rng := RandomNumberGenerator.new()
var spawn_timer := 0.0
var elapsed := 0.0
var kills := 0
var game_over := false

func _ready() -> void:
	rng.randomize()
	player = PlayerScene.new()
	player.position = get_viewport_rect().size / 2.0
	player.died.connect(_on_player_died)
	player.health_changed.connect(_on_health_changed)
	add_child(player)
	hud = HudScene.new()
	add_child(hud)
	hud.update_stats(player.health, player.max_health, kills, elapsed)
	for i in 4:
		spawn_enemy()
	queue_redraw()

func _process(delta: float) -> void:
	if game_over:
		if Input.is_action_just_pressed("restart"):
			get_tree().reload_current_scene()
		return
	elapsed += delta
	spawn_timer -= delta
	if spawn_timer <= 0.0:
		spawn_enemy()
		spawn_timer = maxf(0.35, 1.6 - elapsed * 0.012)
	hud.update_stats(player.health, player.max_health, kills, elapsed)

func spawn_enemy() -> void:
	if game_over or not is_instance_valid(player):
		return
	var enemy := EnemyScene.new()
	var screen := get_viewport_rect().size
	var side := rng.randi_range(0, 3)
	match side:
		0: enemy.position = Vector2(rng.randf_range(30.0, screen.x - 30.0), 30.0)
		1: enemy.position = Vector2(screen.x - 30.0, rng.randf_range(30.0, screen.y - 30.0))
		2: enemy.position = Vector2(rng.randf_range(30.0, screen.x - 30.0), screen.y - 30.0)
		_: enemy.position = Vector2(30.0, rng.randf_range(30.0, screen.y - 30.0))
	enemy.target = player
	enemy.speed += minf(elapsed * 0.8, 90.0)
	enemy.defeated.connect(_on_enemy_defeated)
	add_child(enemy)

func _on_enemy_defeated(at: Vector2) -> void:
	kills += 1
	if kills % 8 == 0 and is_instance_valid(player):
		player.upgrade()
	hud.show_pickup(at, "+1")

func _on_health_changed(value: int, maximum: int) -> void:
	hud.update_stats(value, maximum, kills, elapsed)

func _on_player_died() -> void:
	game_over = true
	hud.show_game_over(kills, elapsed)

func _draw() -> void:
	var size := get_viewport_rect().size
	for x in range(0, int(size.x), 48):
		draw_line(Vector2(x, 0), Vector2(x, size.y), Color(0.08, 0.12, 0.2, 0.32), 1.0)
	for y in range(0, int(size.y), 48):
		draw_line(Vector2(0, y), Vector2(size.x, y), Color(0.08, 0.12, 0.2, 0.32), 1.0)
	draw_rect(Rect2(Vector2(12, 12), size - Vector2(24, 24)), Color(0.13, 0.42, 0.65, 0.65), false, 3.0)

