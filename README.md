# Mini Factory

Мобильный кликер/тайкун (Idle / Incremental Game) на движке **Unity 6 (6000.3.22f1)** под платформу Android (Portrait 9:16). Проект разработан в рамках выполнения тестового задания Unity Developer в соответствии с требованиями [.agents/ТЗ.md](.agents/ТЗ.md).

---

## 1. Инструкция по запуску и сборке

### Требования к окружению
- **Unity Editor:** `6000.3.22f1` (Unity 6.3 LTS).
- **Платформа:** Android (установленный модуль Android Build Support + OpenJDK / Android SDK & NDK Tools).

### Запуск в Unity Editor
1. Откройте проект в Unity Hub с версией `6000.3.22f1`.
2. Откройте сцену `Assets/_Project/Scenes/Main.unity`.
3. В окне **Game** выберите соотношение сторон **9:16 Portrait** (например, `1080x1920` или эмулятор `Google Pixel 7`).
4. Нажмите кнопку **Play**.

### Сборка Android APK
1. **В один клик из меню Unity Editor:**
   - Выберите в верхнем меню: **Tools > Mini Factory > Build Android APK**.
   - Скрипт `BuildScript.cs` автоматически настроит параметры плеера (ARM64, Portrait) и скомпилирует APK в директорию `Builds/Android/MiniFactory.apk`.
2. **Через командную строку (CLI / CI):**
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Unity.exe" -projectPath . -executeMethod MiniFactory.Editor.BuildScript.BuildAndroid -batchmode -quit -logFile Logs/build.log
   ```
3. **Стандартно через Build Settings:**
   - Откройте **File > Build Settings** (или **Build Profiles**).
   - Выберите платформу **Android** и нажмите **Switch Platform**.
   - Убедитесь, что сцена `Assets/_Project/Scenes/Main.unity` добавлена в список.
   - Нажмите **Build** и выберите директорию сохранения APK.

---

## 2. Архитектура и ключевые системы

Архитектура спроектирована по принципам высокой связности внутри модулей и слабой связности между системами, без использования избыточных сторонних DI-фреймворков и без скрытых синглтонов (`GameManager.Instance` запрещены).

```text
                                  [ Bootstrap ] (Composition Root)
                                       │
        ┌───────────────────┬──────────┴──────────┬──────────────────┐
        ▼                   ▼                     ▼                  ▼
   [ GameConfig ]      [ Wallet ]           [ MachineState ]    [ BoostState ]
(IConfigService)     (State Owner)          (State Owners)      (State Owner)
        │                   │                     │                  │
        │             CoinsChanged             Changed          StateChanged
        │                   │                     │                  │
        ▼                   ▼                     ▼                  ▼
[ IStoreService ]      [ WalletUI ]        [ MachineCardUI ]     [ BoostUI ]
        │                   ▲                     ▲                  │
  Unity IAP v5              └─────────────────────┴──────────────────┘
  (Fake Store)                                    │
                                                  ▼
                                       [ IAnalyticsService ]
                                                  │
                                      [ DebugAnalyticsProvider ]
                                          (Console Logger)
```

### Основные компоненты:
1. **Composition Root (`Bootstrap.cs`):**
   - Единственная точка входа на сцене.
   - Загружает сохранения через `SaveService`, инициализирует чистые C#-модели (`Wallet`, `MachineCollection`, `BoostState`).
   - Рассчитывает офлайн-прогресс через чистый калькулятор `OfflineProgressCalculator`.
   - Внедряет зависимости в компоненты представления (`WalletUI`, `BoostUI`, `MachineCardUI`, `MachineWorldView`, `ShopUI`).
   - Управляет игровым циклом начисления валюты с защитой от округления дробных монет на высоких FPS (`_fractionalCoins`).
2. **Single State Owner:**
   - `Wallet`: единственный владелец баланса игрока. Предоставляет методы `TrySpend` и `Add`, оповещает через событие `CoinsChanged`.
   - `MachineState`: хранит уровень, статус открытия, формулы стоимости улучшения и дохода машины.
   - `BoostState`: управляет временем и множителем активного буста фабрики.
3. **Data-Driven Конфигурация (`IConfigService` / `GameConfig`):**
   - Все параметры машин (базовые доходы, стоимости, мультипликаторы) и параметры буста/офлайна вынесены в ScriptableObject `GameConfig.asset`.
   - Домен зависит только от интерфейса `IConfigService`, что позволяет подключить Remote Config (например, Firebase Remote Config) без изменения игровой логики.
4. **Unity In-App Purchasing v5 (`UnityIAPStoreService`):**
   - Интегрирован актуальный пакет `com.unity.purchasing` v5.4.4.
   - Включен режим **Fake Store** (`DefaultStoreHelper.OverrideDefaultStoreName("fake")`).
   - Реализован обязательный двухэтапный флоу подтверждения покупок v5: `OnPurchasePending` -> начисление -> `storeController.ConfirmPurchase(pendingOrder)`.
   - Игровой код и UI изолированы через интерфейс `IStoreService`.
5. **Мультипровайдерная Аналитика (`AnalyticsService`):**
   - Шина аналитики `IAnalyticsService` с возможностью одновременного подключения произвольного количества `IAnalyticsProvider`.
   - Реализован `DebugAnalyticsProvider` для подробного форматированного вывода в консоль.
   - Поддержаны все 8 обязательных событий ТЗ с параметрами:
     - `game_started` (coins, unlocked_machines_count)
     - `machine_unlocked` (machine_id)
     - `machine_upgraded` (machine_id, new_level)
     - `boost_started` (duration, multiplier)
     - `boost_finished`
     - `offline_income_applied` (offline_seconds, offline_income)
     - `purchase_succeeded` (product_id, coins_reward)
     - `purchase_failed` (product_id, reason)
6. **Адаптивная камера (`CameraResolutionFitter.cs`):**
   - Автоматически адаптирует ортографический размер камеры и выставляет pillarboxing при запуске на широкоформатных или нестандартных экранах, сохраняя идеальное кадрирование 9:16 фабрики и UI.
7. **Централизованный аудио-фидбек (`FeedbackRoot.cs`):**
   - Единый компонент `FeedbackRoot` с пулом настроенных аудио-клипов для отклика без хаотичных разрозненных AudioSource:
     - `ButtonClick` — щелчок интерфейса;
     - `MachineUnlock` — торжественный аккорд открытия нового станка;
     - `MachineUpgrade` — сочный звук прокачки уровня;
     - `BoostActive` — динамичное ускорение при активации буста;
     - `PurchaseSuccess` — звон монет при покупке IAP-пака.
   - Подключен через события напрямую в `Bootstrap` и во все интерактивные кнопки UI.
8. **Анимация станков (`MachineWorldView.cs`):**
   - Активные (открытые) машины имеют динамическое синусоидальное покачивание масштаба по оси Y без использования сторонних тяжелых библиотек (DOTween не требуется):
     $$\text{speed} = 4.0 + \text{level} \times 0.3$$
     $$\text{amplitude} = \min(0.04 + \text{level} \times 0.012,\, 0.14)$$
   - Чем выше уровень станка, тем быстрее и выразительнее он качается. Неоткрытые станки затемнены и неподвижны. Нулевые аллокации в кадре.

---

## 3. Использованные пакеты и ассеты

### Unity Packages:
- `com.unity.purchasing` (5.4.4) — Unity In-App Purchasing v5.
- `com.unity.ugui` (2.0.0) — uGUI + TextMeshPro.
- `com.unity.render-pipelines.universal` (17.3.0) — URP 2D.
- `com.unity.inputsystem` (1.20.0) — New Input System.
- `com.unity.test-framework` (1.6.0) — Unity Test Framework (NUnit).

### Графика и аудио:
- **Kenney Tiny Factory:** Аутентичные тайлы станков (`tile_0075.png` бур, `tile_0099.png` плавильня, `tile_0111.png` пресс) и бесшовный пол фабрики (`tile_0000.png`).
- **Kenney Pixel UI:** 9-slice спрайты для плашек (`space.png`, `Ancient/grey.png`) и кнопок (`Colored/green.png`, `Colored/blue.png`, `Colored/yellow.png`).
- **Kenney Future:** Пиксельный ретро-шрифт, импортированный как TextMeshPro SDF Font Asset (`KenneyFuture_SDF.asset`).
- **Kenney Audio (Interface Sounds):** Аудио-клипы в формате OGG Vorbis (`sfx_click`, `sfx_machine_unlock`, `sfx_machine_upgrade`, `sfx_boost`, `sfx_purchase`).

---

## 4. Профайлинг и оптимизация (Android)

- **Target Architecture:** ARM64 (IL2CPP / Android 7.0+ API 25+).
- **SRP Batcher:** Включен в URP 2D, объединяет отрисовку тайлов и спрайтов мира в минимальное число батчей.
- **UI Оптимизация:** 
  - Единый динамический атлас SDF шрифта TextMeshPro (`KenneyFuture_SDF`).
  - Все UI-элементы разбиты по слоям с 9-slice спрайтами без овердроу пустых прозрачных зон.
  - Текстовые поля обновляются строго по событиям (`CoinsChanged`, `Changed`, `StateChanged`), а не каждый кадр в `Update()`.
- **GC Allocations:** 0 B аллокаций в кадре в основном цикле производства и покачивания машин.
- **Audio:** Все SFX сжаты в OGG Vorbis с качеством 100% и предзагрузкой `Preload Audio Data`, воспроизводятся через `AudioSource.PlayOneShot` без спавна временных GameObject.

---

## 4. Unit-тесты

В проекте реализовано **10 EditMode unit-тестов** (`Assets/_Project/Scripts/Tests/GameplayTests.cs`), проверяющих ключевую логику без зависимостей от физики и сцены:
1. `Wallet_TrySpend_RejectsWhenInsufficientCoins` — проверка запрета списания при нехватке монет.
2. `Wallet_TrySpend_SucceedsAndDeductsCoins` — успешное списание монет и вызов события.
3. `MachineState_Unlock_StartsAtLevel1WithBaseIncome` — разблокировка машины, выставление 1 уровня и базового дохода.
4. `MachineState_Upgrade_IncreasesIncomeAndCost` — масштабирование стоимости и дохода при прокачке.
5. `OfflineProgressCalculator_CapsAtMaxSeconds` — ограничение офлайн-производства максимальным лимитом времени (7200с).
6. `OfflineProgressCalculator_AppliesBoostMultiplier` — корректный учет оставшегося времени буста при расчете офлайн-дохода.
7. `BoostState_TicksAndExpiresProperly` — обратный отсчет таймера буста и сброс множителя до 1x по истечении времени.
8. `BoostState_FiresStartedAndFinishedEvents` — вызов событий `BoostStarted` и `BoostFinished` с аргументами.
9. `AnalyticsService_DispatchesToMultipleProviders` — рассылка событий и параметров нескольким провайдерам аналитики одновременно.
10. `Store_SimulatedPurchaseSucceeded_CreditsWallet` — начисление игровой валюты при успешной покупке.

Запуск тестов: **Window > General > Test Runner > EditMode > Run All** или через Unity CLI `-runTests -testPlatform editmode`.

---

## 5. Известные ограничения и следующие шаги

### Ограничения:
- Для IAP используется режим **Fake Store** (эмуляция успешных покупок в редакторе и на устройстве без привязки к Google Play Console).
- В качестве аналитического провайдера подключен консольный `DebugAnalyticsProvider`.

### Следующие шаги (бонусы ТЗ):
1. **Firebase Remote Config:** Реализация `RemoteConfigService : IConfigService`, загружающего баланс машин и буста из облака с фолбэком на локальный `GameConfig`.
2. **AppMetrica / Firebase Analytics:** Создание `AppMetricaAnalyticsProvider : IAnalyticsProvider` и добавление его в `AnalyticsService` в одну строчку кода в `Bootstrap`.
3. **PlayMode UI integration tests:** Автоматизированные тесты полного цикла интерфейса через Unity Test Framework.

---

## 6. Девайсы тестирования и время выполнения

- **Устройство/эмулятор:** Google Pixel 7 (Android Simulator portrait `1080x1920`, DPI 420).
- **Фактическое время выполнения:** ~4.5 часа.
- **Отчет об использовании AI:** См. файл [AI_USAGE.md](AI_USAGE.md).
