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
- `WindowsBuild` — сборка игры в `.exe` + упаковка в `zip`.
- GitHub Actions workflow — автосборка ZIP для релизов.

## Я хочу просто скачать ZIP с GitHub и запустить игру

Да, можно. Целевой поток такой:
1. Ты публикуешь **Release** в GitHub (например, тег `v1.0.0`).
2. GitHub Action собирает Windows билд и прикрепляет `TopDownRunner-Windows.zip` к релизу.
3. Любой пользователь скачивает ZIP из **Releases**, распаковывает и запускает `TopDownRunner.exe`.

После распаковки не нужен Unity Editor — только Windows.

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

## Как собрать EXE и ZIP локально

### Вариант 1: через Unity Editor

1. Открой проект.
2. Добавь сцену в `File -> Build Settings` (должна быть включена галочка).
3. Запусти `Build -> Build Windows EXE`.
4. Получишь:
   - `Build/Windows/TopDownRunner.exe` (и связанные файлы),
   - `Build/Releases/TopDownRunner-Windows.zip` (готов для раздачи).

### Вариант 2: через командную строку (CI / автоматизация)

```bash
"C:/Program Files/Unity/Hub/Editor/<UNITY_VERSION>/Editor/Unity.exe" \
  -quit -batchmode -projectPath "<PATH_TO_PROJECT>" \
  -executeMethod WindowsBuild.BuildFromCommandLine
```

## Как включить автосборку ZIP на GitHub

В репозитории уже есть workflow: `.github/workflows/build-windows-release.yml`.

Нужно добавить GitHub Secrets:
- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

Дальше:
1. Запушь тег `v1.0.0` (или любой `v*`).
2. Запустится workflow `Build Windows Release`.
3. ZIP будет:
   - в `Actions` как artifact,
   - в `Releases` как файл релиза (для тегов `v*`).

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
