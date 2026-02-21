# TopDownRunner (скачал ZIP с GitHub и запустил EXE)

Теперь проект содержит **standalone C# WinForms-игру** (без Unity лицензии), чтобы ты мог:
1. Запушить код в GitHub,
2. Создать тег/релиз,
3. Скачать ZIP из Releases,
4. Запустить `TopDownRunner.exe`.

## Где игра

- Проект: `StandaloneRunner/StandaloneRunner.csproj`
- Главная логика: `StandaloneRunner/RunnerGameForm.cs`
- Workflow сборки: `.github/workflows/build-windows-release.yml`

## Управление

- `← / A` — влево по линии
- `→ / D` — вправо по линии
- `R` — рестарт после проигрыша

## Свои PNG

Если хочешь подставить свои картинки:
- положи `runner.png` и `chaser.png` рядом с `TopDownRunner.exe`
- если файлов нет — используются встроенные простые фигуры.

---

## Пошагово: как получить ZIP на GitHub и запускать EXE

### 1) Создай репозиторий и запушь проект

```bash
git init
git add .
git commit -m "TopDownRunner standalone build"
git branch -M main
git remote add origin https://github.com/<your_user>/<your_repo>.git
git push -u origin main
```

### 2) Создай тег версии

```bash
git tag v1.0.0
git push origin v1.0.0
```

### 3) Дождись завершения GitHub Action

Открой `Actions` → `Build Windows Release (No Unity License)`.

Workflow автоматически:
- собирает Windows EXE через `dotnet publish`,
- запаковывает в `TopDownRunner-Windows.zip`,
- прикладывает ZIP в Releases для тега `v*`.

### 4) Скачай и запусти

- Открой вкладку **Releases** в GitHub.
- Скачай `TopDownRunner-Windows.zip`.
- Распакуй архив.
- Запусти `TopDownRunner.exe`.

Готово — Unity, `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` больше **не нужны**.

## Локальный запуск без GitHub

```bash
dotnet publish StandaloneRunner/StandaloneRunner.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o Build/Windows
```

После этого EXE будет в `Build/Windows/TopDownRunner.exe`.
