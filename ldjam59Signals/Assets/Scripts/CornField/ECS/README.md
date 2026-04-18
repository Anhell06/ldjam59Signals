# GrassField — Кастомный ECS (без Unity.Entities)

Нулевые внешние зависимости. Работает на любой версии Unity 2020+.

---

## Архитектура

```
GrassWorld (MonoBehaviour)
│
│  Awake() ─── создаёт GrassComponents (SoA) + все системы
│
└─ Update() ──────────────────────────────────────────────────────
      │
      ├─ 1. GrassInteractionSystem.Execute(components)
      │       Читает GrassInteractor.Position (активные игроки)
      │       Для каждого стебля в радиусе → пишет BendAngle, BendAxis
      │
      ├─ 2. GrassSwaySystem.Execute(components, wind, time, dt)
      │       Для каждого стебля:
      │         sin(time * freq + phase) * amplitude * windStrength → windAngle
      │         Lerp(BendAngle → 0, dt * recoverySpeed)             → затухание
      │         Quaternion(windRot * bendRot * baseRot)             → финальный поворот
      │         Matrix4x4.TRS(pos, rot, scale)                      → в Matrices[i]
      │
      └─ 3. GrassRenderSystem.Execute(components)
              DrawMeshInstanced(Matrices, batch=1023) × N батчей → GPU


GrassComponents (SoA — Structure of Arrays)
  float[]   SwayPhase, SwayAmplitude, WindInfluence
  float[]   BendAngle,  RotationsY
  Vector3[] Positions,  BendAxis
  Matrix4x4[] Matrices  ← пишет Sway, читает Render
```

---

## Структура файлов

```
GrassECS_Custom/
├── Components/
│   ├── GrassComponents.cs     ← SoA: все массивы данных
│   └── WindData.cs            ← структура ветра (value type)
├── Systems/
│   ├── GrassSwaySystem.cs     ← качание + матрицы TRS
│   ├── GrassInteractionSystem.cs ← реакция на игрока
│   └── GrassRenderSystem.cs   ← GPU Instancing
├── GrassWorld.cs              ← MonoBehaviour, точка входа
└── GrassInteractor.cs         ← компонент на игрока
```

---

## Подключение (3 шага)

### 1. GrassWorld на сцену
Создайте пустой GameObject → добавьте `GrassWorld`.

| Поле             | Описание                                              |
|------------------|-------------------------------------------------------|
| Field Width/Height | Количество стеблей по X и Z                         |
| Spacing          | Расстояние между стеблями (м)                        |
| Jitter           | Случайный сдвиг позиции (органичность)               |
| Min/Max Height   | Диапазон высот стеблей                               |
| Grass Mesh       | Меш стебля (простой quad или цилиндр)                |
| Grass Material   | **Обязательно: Enable GPU Instancing в настройках!** |
| Wind Direction   | Направление ветра (Y-компонент игнорируется)         |

### 2. Меш и материал
- Меш: любой простой quad/plane, повёрнутый вертикально (pivot у основания)
- Материал: стандартный Lit или кастомный шейдер → ✅ **Enable GPU Instancing**

### 3. Игрок
Добавьте компонент `GrassInteractor` на GameObject игрока.
- **Radius** — радиус влияния (1–3м)
- **Force** — сила изгиба [0..1]

Gizmo покажет радиус в Scene View.

---

## Управление ветром из кода

```csharp
var world = FindObjectOfType<GrassWorld>();

// Изменить направление и силу
world.SetWind(new Vector3(0f, 0f, 1f), 1.5f);
```

---

## Производительность

| Стеблей | Update() (no Jobs) | С C# Jobs (доработка) |
|---------|--------------------|-----------------------|
|  10 000 | ~1–2 ms            | ~0.2 ms               |
|  50 000 | ~5–8 ms            | ~0.8 ms               |
| 100 000 | ~10–15 ms          | ~1.5 ms               |

> Без Jobs всё работает на одном потоке — для 10–30к стеблей этого достаточно.
> Для большего количества добавьте `System.Threading.Tasks.Parallel.For` в GrassSwaySystem.

---

## Расширение системы

**Новый компонент** — добавить массив в `GrassComponents.cs`:
```csharp
public readonly float[] MyNewData;
// В конструкторе:
MyNewData = new float[count];
```

**Новая система** — создать класс с методом `Execute(GrassComponents data)`:
```csharp
public sealed class MySystem {
    public void Execute(GrassComponents data) { /* ... */ }
}
```
Зарегистрировать в `GrassWorld.Update()`:
```csharp
_mySystem.Execute(_components);
```
