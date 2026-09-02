using System.IO.Pipes;
using System.Text;
using System.Diagnostics;


namespace tg_bot_anon_chat
{
    class AdminServer()
    {
        private static readonly NamedPipeServerStream server = new("BotPipe", PipeDirection.InOut);
        public static readonly StreamReader reader = new(server, Encoding.UTF8);
        public static readonly StreamWriter writer = new(server, Encoding.UTF8);

        public static async Task StartServer()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = @"..\..\..\..\..\admin_console\admin_console\bin\Debug\net8.0\admin_console.exe",
                UseShellExecute = true
            });

            await server.WaitForConnectionAsync();

            Console.WriteLine("AdminConsole connected...");
        }

        public static bool StartConsoleCommand(CancellationTokenSource cts, DateTime startTime)
        {
            string? consoleCommand = null;
            while (!(consoleCommand == "/start" && cts.IsCancellationRequested))
            {
                consoleCommand = reader.ReadLine();
                switch (consoleCommand)
                {
                    case "/help":
                        writer.WriteLine("/help; /clearlog; /start; /stop; /exit; /status; /reports; /cancelreport");
                        writer.Flush();
                        break;
                    case "/clearlog":
                        Console.Clear();
                        break;

                    case "/start":
                        if (cts.IsCancellationRequested)
                        {
                            writer.WriteLine("Bot is active...");
                            writer.Flush();
                        }
                        else
                        {
                            Console.WriteLine("Command rejected. Bot is active...");
                            writer.WriteLine("Command rejected. Bot is active...");
                            writer.Flush();
                        }
                        break;

                    case "/stop":
                        cts.Cancel();

                        TimeSpan ts = DateTime.Now.Subtract(startTime);
                        Console.WriteLine($"Bot is shutdown... | Runtime: {ts:dd\\.hh\\:mm\\:ss}");
                        writer.WriteLine($"Bot is shutdown... | Runtime: {ts:dd\\.hh\\:mm\\:ss}");
                        writer.Flush();
                        break;

                    case "/exit":
                        cts.Cancel();
                        return false;

                    case "/status":
                        writer.WriteLine("Write userId:");
                        writer.Flush();
                        long userId;
                        if (long.TryParse(reader.ReadLine(), out userId) && DatabaseManager.CheckUserExists(userId))
                        {
                            writer.WriteLine("Write status (Confirmed, Suspected, Banned):");
                            writer.Flush();
                            string? status = reader.ReadLine();
                            if (status == "Confirmed" || status == "Suspected" || status == "Banned")
                            {
                                DatabaseManager.ChangeUserStatus(userId, status ?? "");
                                writer.WriteLine("Successfully.");
                                writer.Flush();
                                break;
                            }
                            writer.WriteLine("Wrong status.");
                            writer.Flush();
                        }
                        else
                        {
                            writer.WriteLine("Wrong user or not exists.");
                            writer.Flush();
                        }
                        break;

                    case "/reports":
                        if (DatabaseManager.CheckReportsExists())
                        {
                            writer.WriteLine(DatabaseManager.GetReports());
                            writer.Flush();
                        }
                        else
                        {
                            writer.WriteLine("No reports.");
                            writer.Flush();
                        }
                        break;

                    case "/cancelreport":
                        if (DatabaseManager.CheckReportsExists())
                        {
                            writer.WriteLine("Write userId:");
                            writer.Flush();
                            if (long.TryParse(reader.ReadLine(), out userId) && DatabaseManager.CheckReportExists(userId))
                            {
                                DatabaseManager.CancelReport(userId);
                                writer.WriteLine("Successfully.");
                                writer.Flush();
                            }
                            else
                            {
                                writer.WriteLine("Wrong user or not exists.");
                                writer.Flush();
                            }
                            
                        }
                        else
                        {
                            writer.WriteLine("No users.");
                            writer.Flush();
                        }
                        break;

                    default:
                        Console.WriteLine("Unknown command.");
                        writer.WriteLine("Unknown command.");
                        writer.Flush();
                        break;
                }
            }

            return true;
        }
    }
}
