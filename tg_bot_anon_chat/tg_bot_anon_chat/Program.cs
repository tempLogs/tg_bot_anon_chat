using Telegram.Bot;
using Telegram.Bot.Polling;
using tg_bot_anon_chat;

namespace tg_bot_anon_chat;

class Program
{
    private static readonly string telegramToken = "";

    static async Task Main()
    {
        await AdminServer.StartServer();
        DatabaseManager.InitializeDB();

        while (true)
        {
            var bot = new TelegramBotClient(telegramToken);
            using var cts = new CancellationTokenSource();
            DateTime startTime = DateTime.Now;

            bot.StartReceiving(
                BotHandler.HandleUpdateAsync,
                BotHandler.HandleErrorAsync,
                new ReceiverOptions { AllowedUpdates = [] },
                cts.Token
            );

            Task startMatchingAsync = OnlineDistributor.StartMatchingAsync(bot, cts);

            await bot.SetMyCommands(
            [
                new Telegram.Bot.Types.BotCommand { Command = "/start", Description = "Начать общение с ботом" },
                new Telegram.Bot.Types.BotCommand { Command = "/rules", Description = "Правила нашего сервиса" },
                new Telegram.Bot.Types.BotCommand { Command = "/search", Description = "Найти собеседника" },
                new Telegram.Bot.Types.BotCommand { Command = "/end", Description = "Завершить сеанс разговора или какой-либо процесс" },
                new Telegram.Bot.Types.BotCommand { Command = "/report", Description = "Сообщить о нарушении" }
            ]);

            Console.WriteLine("Bot is active...");

            if (!AdminServer.StartConsoleCommand(cts, startTime))
            {
                await startMatchingAsync;
                return;
            }
            await startMatchingAsync;
        }
    }
}