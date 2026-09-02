using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using tg_bot_anon_chat;

namespace tg_bot_anon_chat
{
    class BotHandler
    {
        private static readonly string rules = System.IO.File.ReadAllText(@"..\..\..\data\rules.txt");

        public static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update is { Message: { } messageText })
            {
                if (messageText.Text == null) return;
                long chatId = update.Message.Chat.Id;
                UserInfo? user = UserManager.GetUser(chatId);

                if (user != null && user.IsConnected)
                {
                    long? value = user.ConnectedUserId;
                    if (value == null) return;

                    if (user.Status == "Reporting")
                    {
                        if (messageText.Text == "/end")
                            await bot.SendMessage(chatId, "Действие отменено.", cancellationToken: token);
                        else
                        {
                            DatabaseManager.ChangeUserStatus((long)value, "Suspected");
                            DatabaseManager.ReportUser(chatId, (long)value, messageText.Text);
                            await bot.SendMessage(chatId, "Спасибо за помощь в модерировании! Найти нового собеседника \"/search\"?", cancellationToken: token);
                        }

                        user.Status = null;
                        user.ConnectedUserId = null;
                        return;
                    }

                    if (messageText.Text == "/end")
                    {
                        UserManager.DisconnectUsers(chatId);

                        await bot.SendMessage(chatId, "Сеанс разговора завершён. Найти нового собеседника \"/search\"?", cancellationToken: token);
                        await bot.SendMessage(value, "Сеанс разговора завершён. Найти нового собеседника \"/search\"?", cancellationToken: token);
                        Console.WriteLine($"{chatId}-{value} | Date: {update.Message.Date.ToLocalTime()} | Message:");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(messageText.Text);
                        Console.ResetColor();
                        Console.WriteLine($"{chatId} | Date: {DateTime.Now} | Disconected with: {value}");
                        Console.WriteLine($"{value} | Date: {DateTime.Now} | Disconected with: {chatId}");
                        return;
                    }
                    else if (messageText.Text == "/report")
                    {
                        UserManager.DisconnectUsers(chatId);
                        user.ConnectedUserId = value;
                        user.Status = "Reporting";

                        await bot.SendMessage(chatId, "Сеанс разговора завершён.", cancellationToken: token);
                        await bot.SendMessage(value, "Сеанс разговора завершён. Найти нового собеседника \"/search\"?", cancellationToken: token);

                        Console.WriteLine($"{chatId} | Date: {DateTime.Now} | Disconected with: {value}");
                        Console.WriteLine($"{value} | Date: {DateTime.Now} | Disconected with: {chatId}");

                        await bot.SendMessage(chatId, "Вы оставляете жалобу на вашего собеседника. Чтобы отменить введите \"/end\". Опишите проблему:", cancellationToken: token);
                        return;
                    }
                    await bot.SendMessage(value, messageText.Text, cancellationToken: token);
                    DatabaseManager.SaveMessage(chatId, (long)value, messageText.Text);

                    Console.WriteLine($"{chatId}-{value} | Date: {update.Message.Date.ToLocalTime()} | Message:");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(messageText.Text);
                    Console.ResetColor();
                    return;
                }

                Console.WriteLine($"{update.Message.Chat} | Date: {update.Message.Date.ToLocalTime()} | Message:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(messageText.Text);
                Console.ResetColor();

                switch (messageText.Text)
                {
                    case "/start":
                        await bot.SendMessage(chatId, "*Здравствуй\\!* Это анонимный чат, в котором вы можете общаться с другими людьми, не раскрывая свою личность\\. По этому просим вас соблюдать правила нашего чата\\. Чтобы начать общение, напишите \"/rules\" и согласитесь с нашими правилами\\.", parseMode: ParseMode.MarkdownV2, cancellationToken: token);
                        return;

                    case "/rules":
                        await bot.SendMessage(chatId, rules, parseMode: ParseMode.MarkdownV2, replyMarkup: new InlineKeyboardMarkup().AddButtons("✅ Согласиться"), cancellationToken: token);
                        return;

                    case "/search":
                        if (DatabaseManager.CheckUserExists(chatId))
                        {
                            if (DatabaseManager.CheckUserStatus(chatId) != "Banned")
                            {
                                if (OnlineDistributor.CheckUserExists(chatId)) return;

                                await bot.SendMessage(chatId, "Ищем собеседника...\nЧтобы отменить поиск испульзуйте команду \"/end\".", cancellationToken: token);
                                UserManager.AddUser(chatId);
                                OnlineDistributor.AddToQueue(chatId);
                            }
                            else
                                await bot.SendMessage(chatId, "К сожалению вы ограничены в доступе. Пожалуйста уважайте правила сервиса.", cancellationToken: token);
                            return;
                        }
                        else
                            await bot.SendMessage(chatId, "Чтобы найти собеседника, вам необходимо сперва согласиться с нашими правилами \"/rules\".", cancellationToken: token);
                        return;

                    case "/end":
                        if (OnlineDistributor.CheckUserExists(chatId))
                        {
                            OnlineDistributor.RemoveFromQueue(chatId);
                            await bot.SendMessage(chatId, "Поиск отменён. Найти нового собеседника \"/search\"?", cancellationToken: token);
                            return;
                        }
                        return;
                }
            }

            if (update is { CallbackQuery: { } query })
            {
                if (query.Message == null) return;
                long chatId = query.Message.Chat.Id;

                if (DatabaseManager.AddUser(chatId, "Confirmed"))
                {
                    await bot.SendMessage(chatId, "*Подтверждено\\!*", parseMode: ParseMode.MarkdownV2, cancellationToken: token);
                    await bot.SendMessage(chatId, "Вы можете найти собеседника, написав команду \"/search\".", cancellationToken: token);
                    Console.WriteLine($"{chatId} | Date: {DateTime.Now} | Confirmed");
                }
                else
                {
                    await bot.SendMessage(chatId, "*Всё уже подтверждено\\!*", parseMode: ParseMode.MarkdownV2, cancellationToken: token);
                    Console.WriteLine($"{chatId} | Date: {DateTime.Now} | Confirmed again");
                }
            }

        }

        public static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {exception.Message}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}
