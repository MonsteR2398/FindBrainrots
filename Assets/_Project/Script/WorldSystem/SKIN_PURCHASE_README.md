# Система покупки скинов на карте

## Обзор
Система позволяет игрокам покупать скины прямо на карте, подходя к объектам скинов. При подходе игрока открывается UI окно с информацией о скине и возможностью покупки.

## Компоненты системы

### 1. SkinPurchaseUI
UI компонент, который отображает:
- Иконку скина
- Название скина
- Цену и валюту
- Кнопку "Купить"
- Кнопку "Отказаться"

**Автоматически регистрируется в UIRegistry** с именем "SkinPurchase".

### 2. SkinPurchasePoint
Компонент-маркер, который хранит данные о скине на конкретном месте на карте. Вешается на тот же GameObject, где находится модель скина.

### 3. UITrigger
Универсальный триггер, который открывает SkinPurchaseUI когда игрок подходит.

## Настройка

### Шаг 1: Создание UI окна покупки

1. Создайте Canvas в сцене UI (или используйте существующий)
2. Создайте GameObject для окна покупки
3. Добавьте компонент `SkinPurchaseUI`
4. Настройте UI элементы в инспекторе:
   - **Root** - корневой объект окна (изначально скрыт)
   - **Skin Icon** - Image компонент для иконки скина
   - **Skin Name Text** - TMP_Text для названия скина
   - **Interaction Text** - TMP_Text для текста кнопки ("Active", "Select", или цена)
   - **Select Button** - Button для выбора скина (если куплен)
   - **Interaction Button** - Button для действий (Active/Select)
   - **Buy Button Container** - Transform контейнер для кнопок покупки
   - **Buy Button Prefab** - Префаб кнопки покупки (Button, TMP_Text для цены, Image "Icon")
   - **Buy Sprite** - спрайт для активной кнопки покупки
   - **No Enough Currency Sprite** - спрайт когда недостаточно валюты
   - **Unlocked Sprite** - спрайт для купленного скина
   - **Selected Sprite** - спрайт для выбранного скина
   - **Model Preview** - RawImage для 3D превью скина
   - **Skin Shop Render** - ссылка на SkinShopRender (опционально)
   - **Skin Shop Manager** - ссылка на SkinShopManager (опционально)

5. Убедитесь, что UI окно находится в сцене UI, которая загружается вместе с основной сценой

### Шаг 2: Создание объекта скина на карте

1. Создайте GameObject в игровой сцене (манекен, постамент, или просто точка появления)
2. Добавьте Collider и установите его как Trigger
3. Добавьте компонент `SkinPurchasePoint`
4. В `SkinPurchasePoint` назначьте `Skin Data` - перетащите SkinSO ассет
5. Добавьте компонент `UITrigger` на тот же GameObject
6. В `UITrigger` установите:
   - **UI Window Name**: "SkinPurchase"
   - **Open Once**: false (или true если нужно открывать только один раз)

### Пример иерархии:
```
GameObject: SkinDisplay_RedDragon
├── Model_RedDragon (3D модель или спрайт)
├── Collider (Trigger)
├── UITrigger
│   └── UI Window Name: "SkinPurchase"
└── SkinPurchasePoint
    └── Skin Data: RedDragonSkin (SkinSO ассет)
```

### Настройка 3D превью:
Для отображения 3D модели скина в UI:
1. В сцене должен быть компонент `SkinShopRender` (уже есть в проекте)
2. У `SkinShopRender` должна быть камера с RenderTexture
3. В `SkinPurchaseUI` назначьте:
   - **Model Preview** - RawImage UI элемент
   - **Skin Shop Render** - компонент SkinShopRender
4. RenderTexture камеры автоматически отобразится в RawImage

## Как это работает

### Flow:
```
Игрок подходит к скину
         ↓
Срабатывает UITrigger.OnTriggerEnter()
         ↓
UITrigger ищет "SkinPurchase" в UIRegistry
         ↓
Находит SkinPurchaseUI и вызывает Open()
         ↓
SkinPurchaseUI ищет ближайший SkinPurchasePoint
         ↓
Получает SkinSO данные из SkinPurchasePoint
         ↓
Отображает данные в UI (иконка, название, цена)
         ↓
Игрок видит окно с информацией о скине
         ↓
Игрок нажимает "Купить" или "Отказаться"
         ↓
UI закрывается
```

### Регистрация:
`SkinPurchaseUI` автоматически регистрируется при запуске:
```csharp
UIRegistry.Register("SkinPurchase", this);
```

`UITrigger` находит UI по имени:
```csharp
IUIOpenable openableUI = UIRegistry.Get("SkinPurchase");
```

## Создание SkinSO ассета

1. В Project окне правой кнопкой → Create → SkinShop → Skin
2. Заполните поля:
   - **ID** - уникальный идентификатор (например, "skin_red_dragon")
   - **Display Name** - название для отображения
   - **Prefab** - префаб скина для игрока
   - **Icon** - иконка для UI
   - **Currency Price** - массив цен (обычно первая валюта)
   - **Map Only** - включите эту галочку, если скин должен продаваться ТОЛЬКО на карте, а не в основном магазине

## Особенности

### Три состояния скина (как в SkinShop):
1. **Active** (Надето) - скин сейчас надет на персонаже
   - Кнопка "Active" (неактивна, серая)
   
2. **Unlocked** (Куплен) - скин куплен но не надет
   - Кнопка "Select" (активна) - можно надеть
   
3. **Locked** (Не куплен) - скин не куплен
   - Кнопки покупки за разные валюты
   - Кнопка неактивна если недостаточно средств

### Поддержка нескольких валют
- Скин может иметь цену в нескольких валютах (Gold, Diamond и т.д.)
- Для каждой валюты создается отдельная кнопка с иконкой валюты
- Кнопка становится неактивной (серый спрайт), если недостаточно средств
- Кнопка активна (спрайт покупки) только если хватает средств на счету

### Поиск ближайшего скина
`SkinPurchaseUI` ищет ближайший `SkinPurchasePoint` на карте, поэтому:
- Можно иметь несколько скинов на карте
- UI всегда покажет данные того скина, к которому игрок подошел

### 3D превью скина
- Используется существующий компонент `SkinShopRender` для рендеринга 3D модели
- Модель отображается в UI через RenderTexture
- При открытии UI загружается модель текущего скина
- При закрытии UI модель очищается

### Интеграция с SkinShopManager
Система использует существующий `SkinShopManager`:
- Проверяет, куплен ли скин (`IsSkinUnlocked`)
- Покупает скин (`TryDressOrBuy`)
- Автоматически надевает купленный скин

## Пример использования

### Сцена 1: Игровая локация
```
WorldScene:
├── Environment
│   ├── Ground
│   └── Buildings
├── Skins
│   ├── SkinDisplay_RedDragon
│   │   ├── Model_RedDragon
│   │   ├── Collider (Trigger)
│   │   ├── UITrigger
│   │   │   └── UI Window Name: "SkinPurchase"
│   │   └── SkinPurchasePoint
│   │       └── Skin Data: RedDragonSkin
│   └── SkinDisplay_GoldenPhoenix
│       ├── Model_GoldenPhoenix
│       ├── Collider (Trigger)
│       ├── UITrigger
│       │   └── UI Window Name: "SkinPurchase"
│       └── SkinPurchasePoint
│           └── Skin Data: GoldenPhoenixSkin
└── Player
    └── PlayerController
```

### Сцена 2: UI (загружается вместе с игровой)
```
UIScene:
└── Canvas
    └── SkinPurchaseUI
        ├── Root (Panel)
        ├── SkinIcon (Image)
        ├── SkinNameText (TMP_Text)
        ├── PriceContainer (Transform) - контейнер для кнопок цен
        │   ├── PriceButton_1 (создается автоматически)
        │   ├── PriceButton_2 (создается автоматически)
        │   └── ...
        ├── PriceButtonPrefab (префаб кнопки)
        │   ├── Button
        │   ├── PriceText (TMP_Text)
        │   └── Icon (Image)
        ├── ModelPreview (RawImage) - для 3D превью
        └── CancelButton (Button)

Также в сцене должен быть:
└── SkinShopRender
    ├── SpawnTarget (Transform)
    └── RenderCamera (Camera с RenderTexture)
```

## Добавление нового скина на карту

1. Создайте `SkinSO` ассет (правой кнопкой → Create → SkinShop → Skin)
2. Создайте GameObject в сцене для отображения скина
3. Добавьте Collider (Trigger) и UITrigger с именем "SkinPurchase"
4. Добавьте SkinPurchasePoint и назначьте SkinSO
5. Готово!

## Troubleshooting

### Предупреждение: "SkinPurchaseUI not found in UIRegistry"
**Решение:** Убедитесь, что UI сцена загружена перед входом в триггер, и `SkinPurchaseUI` находится в сцене.

### Предупреждение: "No skin data found nearby"
**Решение:** Убедитесь, что на объекте скина есть компонент `SkinPurchasePoint` с назначенным `SkinSO`.

### Предупреждение: "No skin data to display!"
**Решение:** Проверьте, что `SkinPurchasePoint` имеет назначенный `SkinSO` в инспекторе.

### Кнопка "Купить" не появляется
**Решение:** Скин уже куплен. Кнопка скрывается автоматически для купленных скинов.

### Цена не отображается
**Решение:** Убедитесь, что в `SkinSO` заполнен массив `CurrencyPrice` и указана хотя бы одна валюта.

## Зависимости

- `ModularSkinShop` - система скинов (SkinSO, SkinShopManager)
- `Treasures.CurrencySystem` - система валют
- `Treasures.WorldSystem.UIRegistry` - реестр UI
- `Treasures.WorldSystem.IUIOpenable` - интерфейс для UI
- `Treasures.WorldSystem.UITrigger` - универсальный триггер