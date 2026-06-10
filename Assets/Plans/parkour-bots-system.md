# Project Overview
- **Game Title:** FindBrainrots
- **High-Level Concept:** Игра-исследование/коллекционирование, где игрок собирает «Brainrot»-объекты на паркур-картах. Этот план добавляет **ботов**, которые двигаются как Player: бегают, прыгают по платформам и проходят паркур, а не просто катаются по плоскости.
- **Players:** Single player + неигровые боты (ambient/конкуренты).
- **Inspiration / Reference Games:** Roblox obby/паркур-карты (камера и контроллер уже в стиле Roblox).
- **Tone / Art Direction:** Стилизованные «парящие острова» (World_Skylands), яркие платформы-кубы.
- **Target Platform:** StandaloneWindows64 (+ мобильные контролы присутствуют в сцене).
- **Screen Orientation / Resolution:** Landscape.
- **Render Pipeline:** URP (17.4.0), Unity 6000.4.6f1, New Input System (1.19.0).

---

## Принятые проектные решения (значения по умолчанию — можно изменить)
На уточняющие вопросы ответа не было, поэтому зафиксированы обоснованные дефолты:

1. **Навигация — гибрид «граф узлов + прыжковые связи», с заделом под сенсоры.**
   - *Почему:* платформы (`World_Skylands` и т.д.) — это отдельные кубы на разной высоте с провалами между ними; NavMesh **не запечён** нигде, и стандартный `NavMeshAgent` не перепрыгивает разрывы без ручных `NavMeshLink`. Граф узлов даёт детерминированный, надёжный паркур на ваших ручных платформах и не требует NavMesh.
   - *Альтернативы (отклонены как основа):* чистый NavMesh+Links (движение не совпадёт с импульсом Player без гибридного слоя), чисто сенсорный эмерджентный ИИ (высокий риск промахов прыжков, много тюнинга). Сенсорный слой закладывается как опциональное расширение позже.

2. **Движение — рефакторинг `PlayerController` для общего кода.**
   - Добавляем публичные `SetMoveInput(Vector2)` и `RequestJump()`, чтобы и человек (через `PlayerInput`), и мозг бота управляли **одним и тем же** кодом локомоции. Это гарантирует, что боты двигаются *точно как Player* (импульс, двойной прыжок, гравитация, поворот).
   - *Альтернатива (отклонена):* отдельный дублирующий контроллер — риск рассинхрона физики с Player.

3. **Поведение бота — патруль/блуждание по платформам (база) + расширяемость.**
   - Базовый режим: бот ходит между узлами графа, прыгает через провалы — выглядит «живым».
   - Архитектура мозга (`IBotGoal`) рассчитана на лёгкое добавление целей: «гонка к точке», «сбор brainrot» (`BrainrotMapInstance`), «преследование игрока».

> Если какое-то из решений не подходит — отметьте в фидбеке, план будет скорректирован.

---

# Game Mechanics

## Core Gameplay Loop
Боты не меняют основной цикл игрока, а обогащают мир: пока игрок ищет brainrot'ы, по платформам перемещаются боты, проходящие тот же паркур. Боты делают карту живой и (опционально) создают соревновательное давление.

**Цикл логики бота (каждый кадр / тик мозга):**
1. **Цель (Goal):** мозг выбирает целевой узел графа (следующая точка патруля / ближайший brainrot / точка рядом с игроком).
2. **Путь (Path):** A* по графу узлов от текущего узла к целевому возвращает список узлов.
3. **Следование (Locomotion):** бот идёт к следующему узлу через `SetMoveInput()`. Если ребро помечено как **Jump Link**, у края платформы вызывается `RequestJump()` (одиночный или двойной — в зависимости от дистанции/высоты), копируя физику прыжка игрока.
4. **Повтор:** при достижении узла берётся следующий; при достижении цели мозг выбирает новую.

## Controls and Input Methods
- **Игрок:** без изменений в ощущении управления — `PlayerInput` (SendMessages) продолжит работать, просто `OnMove/OnJump` будут вызывать новые публичные методы вместо записи в приватные поля.
- **Боты:** без ввода игрока. Управляются программно через `BotBrain` → `BotLocomotion` → `PlayerController` (общий контроллер) или отдельный `CharacterController`-агент (см. шаг 2).

---

# UI
Игровой UI не требуется для MVP. Для отладки добавляются **Gizmos**:
- Узлы графа — сферы; обычные рёбра — белые линии; прыжковые связи (Jump Links) — жёлтые/оранжевые дуги.
- Текущий путь бота — подсветка зелёным во время Play.
- (Опционально) маленький Gizmo-лейбл над ботом с его текущей целью.

Никаких изменений в `Canvas`/HUD не планируется.

---

# Key Asset & Context

### Существующие файлы (контекст)
- `Assets/_Project/Script/PlayerController/PlayerController.cs` — `CharacterController`-локомоция. Публичные свойства: `CurrentVelocity, CurrentSpeed, HorizontalSpeed, VerticalSpeed, IsGrounded, IsFalling, IsJumping`; методы `Launch(Vector3)`, `Teleport(pos,rot)`. **Приватные:** `moveInput`, `jumpRequested`, `jumpsRemaining`, `maxJumps`. Параметры в сцене: moveSpeed=6, jumpHeight=1.5, gravity=-20, maxJumps=2.
- `Assets/_Project/Script/PlayerController/PlayerAnimator.cs` — читает контроллер и ставит Animator-параметры `Speed, AnimSpeed, VerticalSpeed, IsGrounded`. **Переиспользуется ботом без изменений** (работает через `GetComponentInParent<PlayerController>()`).
- `Assets/_Project/Script/WorldSystem/GameModeManager.cs` — грузит миры аддитивно, телепортирует Player в `WorldSpawnPoint`. Сейчас знает только про `PlayerController`.
- `Assets/_Project/Script/WorldSystem/WorldSpawnPoint.cs` — маркер спавна игрока в мире.
- `Assets/_Project/Script/Pickups/BrainrotMapInstance.cs` — коллекционные цели (для будущей цели «сбор brainrot»).
- Миры: `Assets/_Project/Scenes/Worlds/World_Skylands.unity` (чистый паркур, без brainrot — идеальный тест), `World_Grasslands.unity`, `World_Arena.unity`. Платформы — кубы, layer **Default (0)**, **Untagged**, с `BoxCollider`. Player на layer **Ignore Raycast (2)**, tag `Player`.

### Новые файлы (создаются)
| Файл | Назначение |
|---|---|
| `Assets/_Project/Script/Bots/ParkourNode.cs` | Узел графа на платформе (точка, к которой можно идти). Хранит соседей и прыжковые связи. Gizmos. |
| `Assets/_Project/Script/Bots/ParkourLink.cs` (struct/serializable) | Описание ребра: целевой узел + флаг `isJump` + требуемая сила/тип прыжка. |
| `Assets/_Project/Script/Bots/ParkourGraph.cs` | Контейнер графа в сцене: список узлов, поиск ближайшего узла, A* (`FindPath(from,to)`). |
| `Assets/_Project/Script/Bots/BotLocomotion.cs` | Преобразует «двигаться к точке X / прыгнуть» в вызовы `SetMoveInput()/RequestJump()` общего контроллера. |
| `Assets/_Project/Script/Bots/BotBrain.cs` | Конечный автомат: выбор цели → путь по графу → передача команд `BotLocomotion`. Держит ссылку на `IBotGoal`. |
| `Assets/_Project/Script/Bots/Goals/IBotGoal.cs` | Интерфейс цели: `ParkourNode SelectTarget(BotContext ctx)`. |
| `Assets/_Project/Script/Bots/Goals/PatrolGoal.cs` | Базовая цель: случайный/последовательный обход узлов. |
| `Assets/_Project/Script/Bots/BotSpawner.cs` | Спавнит N ботов в текущем мире у `WorldSpawnPoint`/узлов графа. Интегрируется с `GameModeManager`. |
| `Assets/_Project/Script/Bots/Editor/ParkourGraphEditor.cs` | Инструмент для авторинга: кнопки «добавить узел», «соединить выбранные», «пометить как прыжок», авто-связи по дистанции. |
| `Assets/_Project/Prefabs/Bots/Bot.prefab` | Префаб бота (копия визуала Player: `CharacterController` + контроллер + `PlayerAnimator` + `RenderSkin` + `BotBrain` + `BotLocomotion`). |

### Ключевые сигнатуры (черновик API)
```csharp
// PlayerController.cs — НОВЫЕ публичные методы (рефакторинг шаг 1)
public void SetMoveInput(Vector2 input);   // эквивалент moveInput от OnMove
public void RequestJump();                 // эквивалент jumpRequested от OnJump (с проверкой grounded/jumpsRemaining)
public int  JumpsRemaining => jumpsRemaining; // для логики двойного прыжка у бота

// BotLocomotion.cs
public void MoveTowards(Vector3 worldTarget); // считает направление в плоскости XZ -> SetMoveInput
public void Stop();
public void TryJump(bool useDoubleJump);

// ParkourGraph.cs
public List<ParkourNode> FindPath(ParkourNode from, ParkourNode to); // A* по рёбрам
public ParkourNode ClosestNode(Vector3 pos);

// IBotGoal.cs
public interface IBotGoal { ParkourNode SelectTarget(BotContext ctx); bool IsComplete(BotContext ctx); }
```

---

# Implementation Steps

### Шаг 1 — Рефакторинг `PlayerController` под общий ввод
- **Description:** Добавить публичные `SetMoveInput(Vector2)`, `RequestJump()` и свойство `JumpsRemaining`. Переписать `OnMove/OnJump` так, чтобы они вызывали эти методы (поведение для игрока не меняется). Логику проверки `controller.isGrounded || jumpsRemaining > 0` перенести в `RequestJump()`.
- **Файлы:** `Assets/_Project/Script/PlayerController/PlayerController.cs`.
- **Assigned role:** developer.
- **Dependencies:** None.
- **Parallelizable:** No (фундамент для всего).

### Шаг 2 — Решить, как бот переиспользует контроллер
- **Description:** Бот получает **тот же `PlayerController`** на префабе (рекомендуется — гарантированно идентичная физика), но **без** `PlayerInput`. Управление идёт от `BotLocomotion`. Важная деталь: `PlayerController.Start()` берёт `Camera.main` для камеро-относительного движения. Для бота движение должно быть **мирово-ориентированным**, а не камеро-ориентированным. Поэтому добавить в контроллер флаг `useCameraRelativeMovement` (true для игрока, false для бота) — при false направление берётся из мировых осей. Также учесть слой бота (не `Ignore Raycast`, если будут сенсоры; для графа — не критично).
- **Файлы:** `Assets/_Project/Script/PlayerController/PlayerController.cs` (добавить флаг), `Assets/_Project/Prefabs/Bots/Bot.prefab` (новый).
- **Assigned role:** developer.
- **Dependencies:** Шаг 1.
- **Parallelizable:** No.

### Шаг 3 — Система графа паркура
- **Description:** Создать `ParkourNode`, `ParkourLink`, `ParkourGraph` с A* и поиском ближайшего узла. Узлы — пустые GameObject'ы как дети объекта `ParkourGraph` в сцене мира. Рёбра двунаправленные по умолчанию; прыжковые рёбра помечаются `isJump=true`. Gizmos для визуализации.
- **Файлы:** `ParkourNode.cs`, `ParkourLink.cs`, `ParkourGraph.cs` (в `Assets/_Project/Script/Bots/`).
- **Assigned role:** developer.
- **Dependencies:** None (можно параллельно с шагами 1–2).
- **Parallelizable:** Yes.

### Шаг 4 — Редакторский инструмент авторинга графа
- **Description:** `ParkourGraphEditor` (Custom Editor / EditorWindow): кнопки «Добавить узел в точке клика», «Соединить выбранные узлы», «Пометить ребро как Jump», «Авто-соединить по радиусу» (соседние узлы в пределах дистанции; если разрыв/перепад высоты больше порога — пометить как Jump). Это резко ускоряет расстановку.
- **Файлы:** `Assets/_Project/Script/Bots/Editor/ParkourGraphEditor.cs`.
- **Assigned role:** developer.
- **Dependencies:** Шаг 3.
- **Parallelizable:** No.

### Шаг 5 — Авторинг графа для `World_Skylands`
- **Description:** Используя инструмент из шага 4, расставить узлы на платформах `Platform_Center/North/East`, `Bridge` и прыжковые связи через провалы. Это тестовый полигон (там нет brainrot, чистый паркур).
- **Файлы:** `Assets/_Project/Scenes/Worlds/World_Skylands.unity` (добавление GameObject графа).
- **Assigned role:** developer.
- **Dependencies:** Шаг 4.
- **Parallelizable:** No.

### Шаг 6 — Локомоция бота (`BotLocomotion`)
- **Description:** `MoveTowards(worldTarget)` считает горизонтальный вектор к цели и зовёт `SetMoveInput`. `TryJump`: у края платформы (по приближении к узлу-источнику прыжкового ребра) зовёт `RequestJump()`; для длинных/высоких прыжков — второй `RequestJump()` в апексе (двойной прыжок), сверяясь с `JumpsRemaining`. Учитывать `IsGrounded` для тайминга. Порог «достигнут узел» по горизонтальной дистанции.
- **Файлы:** `Assets/_Project/Script/Bots/BotLocomotion.cs`.
- **Assigned role:** developer.
- **Dependencies:** Шаги 1, 2.
- **Parallelizable:** No.

### Шаг 7 — Мозг бота (`BotBrain` + цели)
- **Description:** `BotBrain` — простой FSM: `GetPath` → `FollowPath` → `ReachedGoal`. Держит `IBotGoal` (по умолчанию `PatrolGoal`). На старте находит `ParkourGraph` в загруженном мире, привязывается к ближайшему узлу. `BotContext` несёт ссылки на граф, локомоцию, трансформ игрока. Реализовать `IBotGoal` + `PatrolGoal`.
- **Файлы:** `BotBrain.cs`, `Goals/IBotGoal.cs`, `Goals/PatrolGoal.cs`.
- **Assigned role:** developer.
- **Dependencies:** Шаги 3, 6.
- **Parallelizable:** No.

### Шаг 8 — Префаб бота + анимация
- **Description:** Собрать `Bot.prefab`: `CharacterController` (те же размеры, что у Player: height 1.5, radius 0.5, center (0,0.8,0), step 0.3, slope 45), `PlayerController` (useCameraRelativeMovement=false, без `PlayerInput`), `BotLocomotion`, `BotBrain`, дочерний визуал по образцу `RenderSkin` + `PlayerAnimator` (он уже общий и заработает через `GetComponentInParent`). Назначить Animator Controller, как у игрока.
- **Файлы:** `Assets/_Project/Prefabs/Bots/Bot.prefab`, ссылки на существующий `RenderSkin`/AnimatorController игрока.
- **Assigned role:** developer.
- **Dependencies:** Шаги 2, 6, 7.
- **Parallelizable:** No.

### Шаг 9 — Спавн и интеграция с `GameModeManager`
- **Description:** `BotSpawner` спавнит N ботов при загрузке мира (подписка на `GameModeManager.WorldChanged`), размещает их на узлах графа/у `WorldSpawnPoint`, при смене мира — деспавнит/телепортирует (по аналогии с игроком). Без жёсткой завязки на изменение `GameModeManager` (через событие `WorldChanged`, которое уже есть). Кол-во ботов — поле в инспекторе.
- **Файлы:** `Assets/_Project/Script/Bots/BotSpawner.cs`, добавление объекта `[BotSystem]` в `MainScene` (персистентный).
- **Assigned role:** developer.
- **Dependencies:** Шаги 5, 8.
- **Parallelizable:** No.

### Шаг 10 (опционально, задел) — Дополнительные цели
- **Description:** Реализовать `CollectBrainrotGoal` (путь к ближайшему активному `BrainrotMapInstance`) и/или `ChasePlayerGoal`. Подключаются заменой `IBotGoal` в `BotBrain` без изменения локомоции/графа.
- **Файлы:** `Assets/_Project/Script/Bots/Goals/CollectBrainrotGoal.cs`, `ChasePlayerGoal.cs`.
- **Assigned role:** developer.
- **Dependencies:** Шаг 7.
- **Parallelizable:** Yes (после шага 7).

---

# Verification & Testing

### Ручные проверки (Play Mode, мир `World_Skylands`)
1. **Базовое движение:** игрок управляется как раньше (импульс, двойной прыжок) — регрессия после рефакторинга шага 1.
2. **Бот идёт по узлам:** заспавненный бот доходит до соседнего узла на той же платформе, не «дёргается» и не проскакивает.
3. **Прыжок через провал:** на прыжковом ребре бот прыгает у края и **приземляется** на целевую платформу. Проверить и одиночный, и двойной прыжок (длинный разрыв).
4. **Цикл патруля:** бот непрерывно обходит граф без застреваний и падений вниз. Дать поработать ~2 минуты.
5. **Анимация:** `PlayerAnimator` у бота переключает бег/прыжок/падение (Speed/VerticalSpeed/IsGrounded меняются).
6. **Смена мира:** через портал/`GameModeManager` сменить мир — боты корректно деспавнятся/переспавниваются, нет «висящих» ботов из прошлого мира.

### Граничные случаи
- Бот падает с платформы (промах прыжка) → должен иметь «recovery»: если `transform.position.y` ниже порога — телепорт на ближайший узел графа (fail-safe), залогировать предупреждение.
- Пустой/отсутствующий граф в мире → `BotSpawner` логирует warning и не спавнит (без NRE).
- Цель недостижима (нет пути в графе) → `BotBrain` выбирает другую цель, не зацикливается.

### Авто-тест (опционально)
- Bootstrap Play Mode тест: заспавнить 1 бота на `World_Skylands`, прогнать ~10 сек, проверить, что бот сменил минимум 2 узла и `y` остался выше порога падения.

### Проверка консоли
- После каждого шага — Unity Console без ошибок компиляции и runtime-исключений (NRE при доступе к `Camera.main` у бота, к графу и т.д.).
