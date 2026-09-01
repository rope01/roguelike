extends CanvasLayer

var stats: Label
var hint: Label
var overlay: ColorRect
var result: Label

func _ready() -> void:
	stats = Label.new()
	stats.position = Vector2(30, 25)
	stats.add_theme_font_size_override("font_size", 22)
	stats.add_theme_color_override("font_color", Color("eaf6ff"))
	add_child(stats)
	hint = Label.new()
	hint.text = "WASD — движение    Мышь — прицел    ЛКМ — огонь"
	hint.position = Vector2(30, 602)
	hint.add_theme_font_size_override("font_size", 17)
	hint.add_theme_color_override("font_color", Color(0.65, 0.78, 0.9, 0.8))
	add_child(hint)

func update_stats(health: int, maximum: int, kills: int, elapsed: float, planet: String, progress: int, goal: int) -> void:
	stats.text = "%s\nСердце: %d/%d     Эхо: %d/%d     Всего нот: %d     %02d:%02d" % [planet, health, maximum, progress, goal, kills, int(elapsed) / 60, int(elapsed) % 60]

func show_game_over(kills: int, elapsed: float) -> void:
	overlay = ColorRect.new()
	overlay.color = Color(0.015, 0.02, 0.05, 0.84)
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(overlay)
	result = Label.new()
	result.text = "ЗАБЕГ ОКОНЧЕН\n\nУничтожено врагов: %d\nВремя: %02d:%02d\n\nНажми R, чтобы начать заново" % [kills, int(elapsed) / 60, int(elapsed) % 60]
	result.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	result.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	result.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	result.add_theme_font_size_override("font_size", 28)
	result.add_theme_color_override("font_color", Color("eaf6ff"))
	add_child(result)

func show_note(_world_position: Vector2) -> void:
	var popup := Label.new()
	popup.text = "♪ мелодия становится сильнее"
	popup.position = Vector2(820, 34)
	popup.add_theme_font_size_override("font_size", 18)
	popup.add_theme_color_override("font_color", Color("ffe66d"))
	add_child(popup)
	var tween := create_tween()
	tween.tween_interval(1.2)
	tween.tween_property(popup, "modulate:a", 0.0, 0.45)
	tween.tween_callback(popup.queue_free)

func show_planet_intro(title: String, subtitle: String) -> void:
	var card := ColorRect.new()
	card.color = Color(0.015, 0.02, 0.06, 0.78)
	card.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(card)
	var label := Label.new()
	label.text = title + "\n\n" + subtitle
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	label.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	label.add_theme_font_size_override("font_size", 27)
	label.add_theme_color_override("font_color", Color("f5e9d0"))
	add_child(label)
	var tween := create_tween()
	tween.tween_interval(2.0)
	tween.tween_property(card, "modulate:a", 0.0, 0.7)
	tween.parallel().tween_property(label, "modulate:a", 0.0, 0.7)
	tween.tween_callback(card.queue_free)
	tween.parallel().tween_callback(label.queue_free)

func show_planet_complete(title: String) -> void:
	var message := Label.new()
	message.text = "Мелодия услышана\n" + title + " остаётся позади…"
	message.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	message.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	message.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	message.add_theme_font_size_override("font_size", 30)
	message.add_theme_color_override("font_color", Color("ffe6a7"))
	add_child(message)
	var tween := create_tween()
	tween.tween_interval(1.6)
	tween.tween_property(message, "modulate:a", 0.0, 0.5)
	tween.tween_callback(message.queue_free)
