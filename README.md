# Mini Factory

Тестовое задание на позицию Unity Developer: 2D idle-игра про фабрику из 3+ машин с прогрессией, offline-доходом, boost, Unity IAP и аналитикой.

## Стек

- Unity **6000.3.22f1**, URP 2D
- Платформа: **Android, portrait-only** (в ТЗ ориентация не задана — выбрана вертикальная: машины расположены сверху вниз, навигация вертикальным скроллом)
- UI: uGUI (Canvas + ScrollRect), временная графика — Kenney
- Unity IAP v5 (consumable `coins_pack_small`, Fake Store для проверки)
- Unity Test Framework — EditMode unit-тесты
- Без DI-контейнеров: Composition Root через `Bootstrap`

## Как открыть

1. Unity Hub → Add project from disk → папка репозитория.
2. Откроется в Unity 6000.3.22f1 (нужен модуль Android Build Support для смены build target).
3. Сцена: `Assets/_Project/Scenes/Main.unity`.

## Структура

Весь пользовательский код, сцены и ассеты изолированы внутри `Assets/_Project/`.
План разработки: [ROADMAP.md](ROADMAP.md). История использования AI: [AI_USAGE.md](AI_USAGE.md).

## Git

- Ветки: `main`, `feature/*`, `fix/*`, `chore/*`.
- Коммиты: Conventional Commits на английском (`feat(gameplay): ...`).
- Файлы и их `.meta` коммитятся вместе.
- Бинарные ассеты (png/wav) — через Git LFS (см. `.gitattributes`).

## Ассеты

- [Kenney Tiny Factory](https://kenney.nl/) (`Assets/_Project/Art/Tiles/`) — машины и окружение, CC0.
- [Kenney Pixel UI Pack](https://kenney.nl/) (`Assets/_Project/Art/UI/`) — временный UI, CC0.
- [Kenney Fonts](https://kenney.nl/) (`kenney_kenney-fonts`) — ретро-шрифты под UI (фаза 8), CC0.

Графика и UI по ТЗ не оцениваются — Kenney используется как читаемый дефолт.

## Сборка Android

1. Unity Hub → Installs → 6000.3.22f1 → Add modules → **Android Build Support** (SDK & NDK Tools, OpenJDK).
2. File → Build Profiles → Android → Switch Platform.
3. Портрет зафиксирован в Player Settings (landscape выключен).
4. Build / Build And Run.
