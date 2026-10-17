# Публикация на itch.io

Страница: https://k0chiko.itch.io/tinywizard

## Сборка

Меню Unity: **Tools → TinyWizard → Build WebGL** и **Build Android APK**. Или из терминала (редактор должен быть закрыт):

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity
$UNITY -batchmode -nographics -quit -projectPath "$PWD" -buildTarget WebGL -executeMethod Quinn.Editor.BuildScripts.BuildWebGL -logFile build_webgl.log
$UNITY -batchmode -nographics -quit -projectPath "$PWD" -buildTarget Android -executeMethod Quinn.Editor.BuildScripts.BuildAndroid -logFile build_android.log
```

Результат: `Builds/WebGL/` и `Builds/Android/TinyWizard.apk` (папка `Builds` в `.gitignore`).

Смена платформы (Android ↔ WebGL) заставляет Unity переимпортировать ассеты. Чтобы не ждать каждый раз, собирайте веб-версию из отдельной копии проекта (`git worktree`).

## Публикация (`butler`)

Один раз: `butler login`. Затем:

```bash
butler push Builds/WebGL k0chiko/tinywizard:html5 --userversion 0.25b
butler push Builds/Android/TinyWizard.apk k0chiko/tinywizard:android --userversion 0.25b
butler status k0chiko/tinywizard
```

На странице проекта на itch.io в настройках загрузки отметьте у канала `html5` галочку **This file will be played in the browser**. Размер окна (embed): 1280×720 или полноэкранный режим.

## Определение устройства

`Quinn.PlatformInfo.IsTouch` (`Application.isMobilePlatform`; в браузере на телефоне тоже `true`):

| | Телефон / планшет | ПК |
|---|---|---|
| Интерфейс | экранные стики и кнопки | скрыты, в паузе подсказка по клавишам |
| Движение | левый стик | WASD |
| Стрельба | правый стик | левая кнопка мыши (удерживать), прицел по курсору |
| Спецатака | кнопка с молнией | правая кнопка мыши или `C` |
| Рывок | кнопка со стрелками | пробел |
| Пауза | кнопка паузы | `Esc` или `P` |

Проверка мобильного интерфейса в редакторе: **Tools → TinyWizard → Force Touch UI In Play Mode** (переключатель, меняется вне Play Mode).
