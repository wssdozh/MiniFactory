# ROADMAP — Mini Factory

Каждая фаза — отдельная ветка `feature/*` с критерием выхода. Ничего сверх фазы не делаем.

## Фаза 1 — База (текущая)

Структура папок, правила `.agents/`, README/AI_USAGE/ROADMAP, `.gitignore`, portrait + package id, импорт Kenney, Tile Palette, сцена `Main` с тайловым фоном.

**Выход:** проект открывается без ошибок, всё в git, нет orphan `.meta`.

## Фаза 2 — Ядро архитектуры

- `_Project.Runtime.asmdef`.
- `Bootstrap` (composition root): создаёт сервисы, раздаёт зависимости через `Initialize`.
- Чистые C# (без Unity-зависимостей, тестируемые): `Wallet`, `MachineState`/`MachineCollection`, `BoostState`, `OfflineProgressCalculator`.
- `MachineConfig` (SO) + 3 конфига машин. SO в рантайме не мутируем.
- `SaveService`: JSON в `persistentDataPath`, атомарная запись (temp+replace), `version` в схеме.

**Выход:** домен покрыт типами, всё компилируется, конфиги в `Configs/`.

## Фаза 3 — Gameplay-экран

- Вертикальный `ScrollRect`, карточки машин сверху вниз: базовая открыта по умолчанию, остальные locked.
- Unlock/upgrade за монеты, доход по времени, события-факты (`MachineUnlocked`, `MachineUpgraded`).
- Префабы машин на тайловой сцене; камера/скролл по вертикали.

**Выход:** играбельный цикл: доход → апгрейд → открытие следующей машины.

## Фаза 4 — Analytics

- `IAnalyticsService` + `DebugAnalyticsService` (Console). Gameplay знает только интерфейс.
- События ТЗ: `game_started`, `machine_unlocked`, `machine_upgraded`, `boost_started`, `boost_finished`, `offline_income_applied`, `purchase_succeeded`, `purchase_failed`.

**Выход:** события видны в Console, gameplay-код не ссылается на SDK.

## Фаза 5 — Boost и offline progress

- Boost: активация за монеты, множитель к доходу, время жизни. Учитывается и при offline-расчёте (доля времени под boost).
- Offline income при старте: cap по времени, показ результата игроку.

**Выход:** relog через N минут даёт корректный доход, тесты на cap и boost-долю.

## Фаза 6 — IAP

- Unity IAP v5, consumable `coins_pack_small`, Fake Store.
- Покупка начисляет монеты через `Wallet` (один владелец баланса), идемпотентная обработка результата.

**Выход:** покупка в Fake Store начисляет монеты ровно один раз, `purchase_succeeded/failed` в analytics.

## Фаза 7 — Тесты

Минимум 3 EditMode unit-теста (ТЗ): расчёт production за elapsed time; offline progress с cap и boost; unlock/upgrade (state + событие ровно один раз).

**Выход:** тесты зелёные в Test Runner и batchmode.

## Фаза 8 — UI-полировка и feedback (в конце, графика не оценивается)

- Шрифты: импорт и настройка пиксельного шрифта из `kenney_kenney-fonts` (например `Kenney Pixel.ttf` / `Kenney Future.ttf`) для аккуратного UI в едином ретро-стиле.
- Kenney Pixel UI: панели, кнопки (9-slice), иконки.
- `FeedbackRoot`: сериализованный `List<FeedbackEntry>` (id → animation + AudioClip). Единый вызов `Play(id)` по доменным событиям. Анимации — Animator/корутины, без DOTween.
- Пара SFX (`sfx_*.wav`) на unlock/purchase/boost.

**Выход:** действия игрока дают визуально-звуковой отклик через один компонент.

## Фаза 9 — Android build и README-финал

- Сборка APK/AAB, прогон на устройстве, базовый профайлинг (Profiler), замеры в README.
- Чистка предупреждений, финальный ребейз истории.

**Выход:** стабильная сборка, README отражает реальность.

## Опционально (бонусы ТЗ, только после фазы 9)

- Firebase Remote Config с локальным fallback.
- Firebase Analytics / AppMetrica как второй провайдер `IAnalyticsService`.
- Расширенные тесты и профилирование.
