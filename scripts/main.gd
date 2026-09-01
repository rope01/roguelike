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
var transitioning := false
var current_planet := 0
var planet_kills := 0

const PLANETS := [
	{
		"name": "Планета увядших роз",
		"subtitle": "Здесь цветы помнят песни, которые им не сыграли",
		"sky": Color("11152f"), "ground": Color("342044"), "rim": Color("e778a5"),
		"enemy": Color("ef6a94"), "enemy_speed": 1.0, "enemy_health": 50, "goal": 8
	},
	{
		"name": "Луна сломанных часов",
		"subtitle": "Время идёт, но никого не ждёт",
		"sky": Color("101d31"), "ground": Color("26384b"), "rim": Color("f0bf68"),
		"enemy": Color("e6a84f"), "enemy_speed": 1.18, "enemy_health": 65, "goal": 12
	},
	{
		"name": "Планета немых птиц",
		"subtitle": "Они разучились петь — музыкант должен напомнить им мелодию",
		"sky": Color("081e28"), "ground": Color("163f45"), "rim": Color("75d5c7"),
		"enemy": Color("62c7b8"), "enemy_speed": 1.32, "enemy_health": 80, "goal": 16
	}
]

func _ready() -> void:
	rng.randomize()
	player = PlayerScene.new()
	player.position = get_viewport_rect().size / 2.0
	player.died.connect(_on_player_died)
	player.health_changed.connect(_on_health_changed)
	add_child(player)
	hud = HudScene.new()
	add_child(hud)
	hud.show_planet_intro(PLANETS[current_planet].name, PLANETS[current_planet].subtitle)
	hud.update_stats(player.health, player.max_health, kills, elapsed, PLANETS[current_planet].name, planet_kills, PLANETS[current_planet].goal)
	for i in 3:
		spawn_enemy()
	queue_redraw()

func _process(delta: float) -> void:
	if game_over:
		if Input.is_action_just_pressed("restart"):
			get_tree().reload_current_scene()
		return
	if transitioning:
		return
	elapsed += delta
	spawn_timer -= delta
	if spawn_timer <= 0.0:
		spawn_enemy()
		spawn_timer = maxf(0.35, 1.6 - elapsed * 0.012)
	hud.update_stats(player.health, player.max_health, kills, elapsed, PLANETS[current_planet].name, planet_kills, PLANETS[current_planet].goal)

func spawn_enemy() -> void:
	if game_over or transitioning or not is_instance_valid(player):
		return
	var enemy := EnemyScene.new()
	var screen := get_viewport_rect().size
	var angle := rng.randf_range(0.0, TAU)
	enemy.position = screen / 2.0 + Vector2.from_angle(angle) * 270.0
	enemy.target = player
	var planet: Dictionary = PLANETS[current_planet]
	enemy.configure(planet.enemy, planet.enemy_speed, planet.enemy_health, current_planet)
	enemy.speed += minf(elapsed * 0.55, 70.0)
	enemy.defeated.connect(_on_enemy_defeated)
	add_child(enemy)

func _on_enemy_defeated(at: Vector2) -> void:
	kills += 1
	planet_kills += 1
	hud.show_note(at)
	if planet_kills >= int(PLANETS[current_planet].goal):
		travel_to_next_planet()

func travel_to_next_planet() -> void:
	transitioning = true
	for enemy in get_tree().get_nodes_in_group("enemies"):
		enemy.queue_free()
	player.set_physics_process(false)
	hud.show_planet_complete(PLANETS[current_planet].name)
	await get_tree().create_timer(2.2).timeout
	current_planet = (current_planet + 1) % PLANETS.size()
	planet_kills = 0
	spawn_timer = 1.0
	player.position = get_viewport_rect().size / 2.0
	player.upgrade()
	player.set_physics_process(true)
	transitioning = false
	queue_redraw()
	hud.show_planet_intro(PLANETS[current_planet].name, PLANETS[current_planet].subtitle)
	for i in 3:
		spawn_enemy()

func _on_health_changed(value: int, maximum: int) -> void:
	hud.update_stats(value, maximum, kills, elapsed, PLANETS[current_planet].name, planet_kills, PLANETS[current_planet].goal)

func _on_player_died() -> void:
	game_over = true
	hud.show_game_over(kills, elapsed)

func _draw() -> void:
	var size := get_viewport_rect().size
	var planet: Dictionary = PLANETS[current_planet]
	draw_rect(Rect2(Vector2.ZERO, size), planet.sky)
	for i in 42:
		var star := Vector2(float((i * 83 + 47) % int(size.x)), float((i * 137 + 29) % int(size.y)))
		if star.distance_to(size / 2.0) > 292.0:
			draw_circle(star, 1.0 + float(i % 3) * 0.55, Color(0.84, 0.9, 1.0, 0.35 + float(i % 4) * 0.13))
	var center := size / 2.0
	draw_circle(center + Vector2(12, 18), 278.0, Color(0.0, 0.0, 0.0, 0.28))
	draw_circle(center, 274.0, planet.ground)
	draw_circle(center, 274.0, planet.rim, false, 5.0)
	if current_planet == 0:
		for i in 9:
			var p := center + Vector2.from_angle(float(i) * 0.72) * (110.0 + float(i % 3) * 45.0)
			draw_line(p, p + Vector2(0, 18), Color("6f9b62"), 2.0)
			draw_circle(p, 6.0, Color("bd517a"))
	elif current_planet == 1:
		for i in 7:
			var p := center + Vector2.from_angle(float(i) * 0.91) * (105.0 + float(i % 2) * 70.0)
			draw_circle(p, 19.0, Color(0.1, 0.15, 0.2, 0.45), false, 3.0)
			draw_line(p, p + Vector2(0, -12), Color("d4ae68"), 2.0)
			draw_line(p, p + Vector2(9, 5), Color("d4ae68"), 2.0)
	else:
		for i in 12:
			var p := center + Vector2.from_angle(float(i) * 0.54) * (105.0 + float(i % 4) * 35.0)
			draw_arc(p, 8.0, PI, TAU, 8, Color("82cfc5"), 2.0)
			draw_arc(p + Vector2(15, 0), 8.0, PI, TAU, 8, Color("82cfc5"), 2.0)
