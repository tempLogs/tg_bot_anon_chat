using Telegram.Bot;

namespace tg_bot_anon_chat
{
    class OnlineDistributor
    {
        private static readonly Random random = new();
        private static readonly List<long> queue = [];

        public static void AddToQueue(long userId)
        {
            if (!queue.Contains(userId))
                queue.Add(userId);
        }

        public static bool CheckUserExists(long userId)
        {
            if (queue.Contains(userId)) return true;

            return false;
        }

        public static void RemoveFromQueue(long userId)
        {
            if (queue.Contains(userId))
                queue.Remove(userId);
        }

        public static async Task StartMatchingAsync(TelegramBotClient bot, CancellationTokenSource cts)
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    if (queue.Count < 2)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    long firstUser = queue[0];
                    long secondUser = queue[random.Next(1, queue.Count)];

                    queue.Remove(secondUser);
                    queue.Remove(firstUser);

                    UserManager.ConnectUsers(firstUser, secondUser);

                    await bot.SendMessage(firstUser, "Собеседник найден! \"/end\" - завершить.");
                    await bot.SendMessage(secondUser, "Собеседник найден! \"/end\" - завершить.");
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}
