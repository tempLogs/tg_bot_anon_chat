using Telegram.Bot;
using Telegram.Bot.Polling;

namespace tg_bot_anon_chat;

class Program
{
    private static readonly string telegramToken = "";

    static async Task Main()
    {
        if (string.IsNullOrWhiteSpace(telegramToken))
        {
            Console.Error.WriteLine("Set telegramToken in the bot's Program.cs before starting.");
            return;
        }

        await AdminServer.StartServer();
        DatabaseManager.InitializeDB();

        while (true)
        {
            var bot = new TelegramBotClient(telegramToken);
            using var cts = new CancellationTokenSource();
            DateTime startTime = DateTime.Now;

            try
            {
                await bot.SetMyCommands(
                [
                    new Telegram.Bot.Types.BotCommand { Command = "start", Description = "Начать общение с ботом" },
                    new Telegram.Bot.Types.BotCommand { Command = "rules", Description = "Правила нашего сервиса" },
                    new Telegram.Bot.Types.BotCommand { Command = "search", Description = "Найти собеседника" },
                    new Telegram.Bot.Types.BotCommand { Command = "end", Description = "Завершить сеанс разговора или какой-либо процесс" },
                    new Telegram.Bot.Types.BotCommand { Command = "report", Description = "Сообщить о нарушении" }
                ], cancellationToken: cts.Token);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.Error.WriteLine($"Telegram rejected startup ({ex.ErrorCode}): {ex.Message}");
                return;
            }
            catch (HttpRequestException)
            {
                Console.Error.WriteLine("Cannot connect to Telegram. Check the network and try again.");
                return;
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine("Telegram startup request was canceled or timed out.");
                return;
            }

            bot.StartReceiving(
                BotHandler.HandleUpdateAsync,
                BotHandler.HandleErrorAsync,
                new ReceiverOptions { AllowedUpdates = [] },
                cts.Token
            );

            Task startMatchingAsync = OnlineDistributor.StartMatchingAsync(bot, cts);

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
