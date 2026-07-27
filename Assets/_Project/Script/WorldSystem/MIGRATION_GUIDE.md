# Руководство по миграции: PortalTrigger → UITrigger

## Обзор
Компонент `PortalTrigger` был заменен на универсальный компонент `UITrigger` с системой регистрации UI. Так как UI находится в другой сцене и загружается во время игры, используется центральный реестр `UIRegistry` для связи триггеров с UI компонентами.

## Что изменилось

### Новые созданные файлы
1. **`IUIOpenable.cs`** - Интерфейс для UI компонентов, которые могут быть открыты
2. **`UIRegistry.cs`** - Центральный реестр для регистрации UI компонентов во время выполнения
3. **`UITrigger.cs`** - Универсальный триггерный компонент, заменяющий PortalTrigger

### Измененные файлы
1. **`PortalWindowUI.cs`** - Теперь реализует `IUIOpenable` и автоматически регистрируется в UIRegistry
2. **`SettingsPanelController.cs`** - Теперь реализует `IUIOpenable` и автоматически регистрируется в UIRegistry

## Как работает новая система

### Архитектура
```
Сцена 1 (World):           Сцена 2 (UI):
┌─────────────────┐        ┌──────────────────┐
│  UITrigger      │        │ PortalWindowUI   │
│  (на триггере)  │        │ SettingsPanel    │
│                 │        │                  │
│  uiWindowName   │        │  - Регистрируют  │
│ = "PortalWindow"│◄───────┤  - себя в        │
└─────────────────┘        │    UIRegistry    │
                           └──────────────────┘
                                    │
                            ┌───────┴────────┐
                            │  UIRegistry    │
                            │  (статика)     │
                            └────────────────┘
```

### Регистрация UI компонентов
UI компоненты автоматически регистрируются в `UIRegistry` при запуске:

```csharp
// В PortalWindowUI.cs
private void Awake()
{
    // ... существующий код ...
    
    // Регистрация в реестре
    UIRegistry.Register("PortalWindow", this);
}

private void OnDestroy()
{
    // Удаление из реестра при уничтожении
    UIRegistry.Unregister("PortalWindow");
}
```

## Шаги миграции

### Шаг 1: Обновление префабов/игровых объектов
Для каждого GameObject, у которого есть компонент `PortalTrigger`:

1. Выберите GameObject в иерархии
2. Удалите компонент `PortalTrigger`
3. Добавьте компонент `UITrigger`
4. В инспекторе компонента `UITrigger`:
   - В поле `UI Window Name` введите имя окна (например, "PortalWindow")
   - Установите флаг `Open Once` если нужно (такое же поведение как раньше)

### Пример: Миграция Portal.prefab
`Portal.prefab` в настоящее время использует `PortalTrigger`:

**До:**
```
GameObject: Portal
└── PortalTrigger
    ├── Open Once: false
    └── (автоматически ссылается на PortalWindowUI.Instance)
```

**После:**
```
GameObject: Portal
└── UITrigger
    ├── UI Window Name: "PortalWindow"
    └── Open Once: false
```

### Шаг 2: Настройка UI компонентов

UI компоненты должны:
1. Реализовать интерфейс `IUIOpenable`
2. Иметь публичный метод `void Open()`
3. Зарегистрировать себя в `UIRegistry` в методе `Awake()`

**Пример для PortalWindowUI:**
```csharp
public class PortalWindowUI : MonoBehaviour, IUIOpenable
{
    private void Awake()
    {
        // ... существующий код ...
        
        // Регистрация в реестре
        UIRegistry.Register("PortalWindow", this);
    }

    private void OnDestroy()
    {
        UIRegistry.Unregister("PortalWindow");
    }

    public void Open()
    {
        // ... существующий код открытия ...
    }
}
```

### Шаг 3: Использование UITrigger

В инспекторе `UITrigger`:
1. В поле `UI Window Name` введите точное имя, зарегистрированное в UI компоненте
2. При входе игрока в триггер автоматически найдет UI в реестре и вызовет `Open()`

## Преимущества новой системы

1. **Поддержка разных сцен**: UI может быть в другой сцене, триггер автоматически найдет его во время игры
2. **Многократное использование**: Один триггерный компонент работает с любым UI
3. **Гибкость**: Легко добавлять новые UI окна без изменения кода триггера
4. **Безопасность типов**: Интерфейс `IUIOpenable` обеспечивает проверку на этапе компиляции
5. **Поддерживаемость**: Один универсальный триггер вместо множества специализированных

## Создание нового UI окна

Чтобы создать новое UI окно, которое можно открыть через триггер:

1. Создайте UI компонент и реализуйте `IUIOpenable`:
```csharp
using Treasures;

public class MyCustomUI : MonoBehaviour, IUIOpenable
{
    private void Awake()
    {
        // Регистрируем в реестре с уникальным именем
        UIRegistry.Register("MyCustomWindow", this);
    }

    private void OnDestroy()
    {
        UIRegistry.Unregister("MyCustomWindow");
    }

    public void Open()
    {
        // Логика открытия окна
        gameObject.SetActive(true);
    }
}
```

2. На триггере добавьте `UITrigger` и укажите имя:
```
UITrigger:
└── UI Window Name: "MyCustomWindow"
```

## Удаление PortalTrigger (опционально)

После миграции всех префабов на `UITrigger`, можно удалить `PortalTrigger.cs`:

```bash
# Сначала сделайте бэкап, затем удалите
del Assets/_Project/Script/WorldSystem/PortalTrigger.cs
```

## Устранение неполадок

### Предупреждение: "UI window 'XXX' not found in UIRegistry"
**Решение:** 
- Убедитесь, что UI компонент зарегистрирован с таким же именем
- Проверьте, что UI компонент находится в сцене во время игры
- Убедитесь, что имя в `UITrigger` точно совпадает с именем при регистрации

### Предупреждение: "UI component does not implement IUIOpenable"
**Решение:** Добавьте `, IUIOpenable` в объявление класса UI компонента.

### UI не открывается при триггере
**Решение:**
- Проверьте, что у игрока установлен тег "Player"
- Убедитесь, что коллайдер установлен как триггер
- Проверьте консоль на наличие ошибок регистрации
- Убедитесь, что UI сцена загружена во время игры

## Доступные UI окна

Текущие зарегистрированные UI окна:
- **"PortalWindow"** - `PortalWindowUI` (выбор мира)
- **"SettingsPanel"** - `SettingsPanelController` (настройки)

Для добавления нового UI окна просто зарегистрируйте его в `UIRegistry` с уникальным именем.
