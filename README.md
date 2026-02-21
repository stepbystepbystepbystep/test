# 2D Top-Down Runner (Subway Surfers style)

Готовый набор C#-скриптов для Unity под мобильный/ПК раннер:
- 3 полосы движения (влево/центр/вправо),
- автоматический бег вперед,
- преследователь сзади,
- препятствия,
- ранец (джетпак), который дает временный полет/неуязвимость к препятствиям.

## Что уже реализовано

- `PlayerRunner` — движение по трем линиям, ускорение, подбор ранца.
- `GameManager` — счет, проигрыш, рестарт (`R`), контроль преследователя.
- `LaneObjectSpawner` — генерация препятствий и ранцев по линиям + очистка объектов позади игрока.
- `TopDownCameraFollow` — камера сверху, следящая за игроком.
- `WindowsBuild` — сборка игры в `.exe` (Windows x64).

## Быстрый запуск в Unity

1. Создай **2D Core** проект в Unity.
2. Скопируй папку `Assets/Scripts` и `Assets/Editor` в проект.
3. На сцене создай объекты:
   - `GameManager` (повесь `GameManager.cs`),
   - `Player` (Sprite + Rigidbody2D + Collider2D + `PlayerRunner.cs`),
   - `Chaser` (Sprite + Collider2D по желанию),
   - `Spawner` (`LaneObjectSpawner.cs`),
   - `Main Camera` (`TopDownCameraFollow.cs`).
4. В `GameManager` привяжи:
   - `Player`,
   - `Chaser`,
   - `ScoreText` (TMP_Text, опционально),
   - `GameOverPanel` (UI панель, опционально).
5. В `LaneObjectSpawner` привяжи:
   - `Player`,
   - массив `Obstacle Prefabs` (теги можно не ставить вручную, скрипт ставит `Obstacle`),
   - `Jetpack Prefab` (скрипт ставит `Jetpack`).
6. Вставь свои PNG:
   - спрайт бегущего персонажа на `Player`,
   - спрайт преследователя на `Chaser`,
   - спрайты препятствий и ранца в соответствующие префабы.

## Управление

- `← / A` — смена полосы влево,
- `→ / D` — смена полосы вправо,
- `R` — перезапуск после проигрыша.

## Как собрать EXE

### Вариант 1: через Unity Editor

1. Открой проект.
2. Добавь сцену в `File -> Build Settings` (должна быть включена галочка).
3. Запусти `Build -> Build Windows EXE`.
4. Готовый файл будет в `Build/Windows/TopDownRunner.exe`.

### Вариант 2: через командную строку (CI / автоматизация)

```bash
"C:/Program Files/Unity/Hub/Editor/<UNITY_VERSION>/Editor/Unity.exe" \
  -quit -batchmode -projectPath "<PATH_TO_PROJECT>" \
  -executeMethod WindowsBuild.BuildFromCommandLine
```

После успешной сборки EXE появится в `Build/Windows/TopDownRunner.exe`.

## Как выгрузить на GitHub

```bash
git init
git add .
git commit -m "Initial Unity top-down runner"
git branch -M main
git remote add origin https://github.com/<your_user>/<your_repo>.git
git push -u origin main
```

## Важные детали

- Для столкновений у префабов препятствий/ранца должны быть `Collider2D` с `Is Trigger = true`.
- У игрока должен быть `Rigidbody2D` (скрипт сам отключает гравитацию).
- Если хочешь эффект "полета выше", можно в `ActivateJetpack()` добавить анимацию масштаба/сортировки слоя.
