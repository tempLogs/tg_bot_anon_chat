# Telegram Anonymous Chat Bot

Учебный проект — Telegram-бот для анонимного общения между случайными
пользователями.

Проект был создан для практики разработки на C#/.NET, работы с Telegram
Bot API, асинхронным программированием, SQLite и межпроцессным
взаимодействием.

## Features

- 🔎 Поиск случайного собеседника
- 💬 Анонимная пересылка сообщений
- 🔌 Подключение и отключение пользователей от сессии
- 🚨 Система жалоб на собеседников
- 📜 Хранение пользователей, сообщений и жалоб в SQLite
- 🛠️ Отдельная административная консоль
- 🔗 Взаимодействие бота и admin console через Named Pipes
- 📊 Просмотр состояния бота из административной консоли
- ⏹️ Управление запуском и остановкой сервиса
- 📋 Логирование событий работы бота

## Commands

### User commands

|  Command  |            Description           |
|-----------|----------------------------------|
| `/start`  | Запустить взаимодействие с ботом |
| `/rules`  | Показать правила сервиса         |
| `/search` | Найти случайного собеседника     |
| `/end`    | Завершить текущую сессию         |
| `/report` | Пожаловаться на собеседника      |

### Admin commands

|     Command     |            Description           |
|-----------------|----------------------------------|
| `/help`         | Показать список доступных команд |
| `/clearlog`     | Очистить консольный лог          |
| `/start`        | Запустить работу бота            |
| `/stop`         | Остановить работу бота           |
| `/exit`         | Завершить приложение             |
| `/status`       | Изменить статус пользователя     |
| `/reports`      | Просмотреть жалобы               |
| `/cancelreport` | Отменить обработку жалобы        |

## Architecture

Проект разделён на два .NET-приложения:

```text
tg_bot_anon_chat/
│
├── tg_bot_anon_chat/
│   ├── Program.cs
│   ├── BotHandler.cs
│   ├── OnlineDistributor.cs
│   ├── UserManager.cs
│   ├── UserInfo.cs
│   ├── DatabaseManager.cs
│   ├── AdminServer.cs
│   └── data/
│       ├── database.db
│       └── rules.txt
│
└── admin_console/
    └── Program.cs
```


BotHandler:
Обрабатывает сообщения и команды пользователей Telegram.

OnlineDistributor:
Отвечает за поиск собеседников.
Пользователи, ожидающие собеседника, помещаются в очередь. После
нахождения пары между пользователями устанавливается соединение.

UserManager:
Управляет состоянием пользователей и их текущими соединениями.

DatabaseManager:
Отвечает за работу с SQLite и хранение:
пользователей;
сообщений;
жалоб.

AdminServer:
Предоставляет интерфейс взаимодействия с административной консолью
через Named Pipes.


## Tech Stack

- C#
- .NET 8
- Telegram Bot API
- Telegram.Bot
- SQLite
- Asynchronous Programming (`async`/`await`)
- Named Pipes (IPC)


## Getting Started

### Prerequisites

- .NET 8 SDK
- Windows
- A Telegram Bot Token

### Configuration

Для запуска необходимо указать в файле tg_bot_anon_chat/tg_bot_anon_chat/Program.cs свой токен Telegram-бота
*примечание*: никогда никому не передавайте свой токен

### Build

```bash
dotnet build
