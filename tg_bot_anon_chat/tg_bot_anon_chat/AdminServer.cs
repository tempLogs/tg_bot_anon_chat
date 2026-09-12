using System.IO.Pipes;
using System.Text;
using System.Diagnostics;


namespace tg_bot_anon_chat
{
    class AdminServer()
    {
        private static NamedPipeServerStream server = new("BotPipe", PipeDirection.InOut);
        public static StreamReader reader = new(server, Encoding.UTF8, leaveOpen: true);
        public static StreamWriter writer = new(server, Encoding.UTF8, leaveOpen: true);

        public static async Task StartServer()
        {
            string consolePath = @"..\..\..\..\..\admin_console\admin_console\bin\Debug\net8.0\admin_console.exe";
            if (File.Exists(consolePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = consolePath,
                        UseShellExecute = true
                    });
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    Console.WriteLine($"Cannot start admin console: {ex.Message}. Start it manually.");
                }
            }
            else
            {
                Console.WriteLine("Admin console executable was not found. Start the console manually.");
            }

            Console.WriteLine("Waiting for admin console...");
            await server.WaitForConnectionAsync();

            Console.WriteLine("AdminConsole connected...");
        }

        public static bool StartConsoleCommand(CancellationTokenSource cts, DateTime startTime)
        {
            while (true)
            {
                try
                {
                    return ReadConsoleCommands(cts, startTime);
                }
                catch (IOException)
                {
                    Console.WriteLine("Admin console disconnected.");
                    ReconnectConsole();
                }
            }
        }

        private static void ReconnectConsole()
        {
            try
            {
                writer.Dispose();
            }
            catch (IOException)
            {

            }
            finally
            {
                reader.Dispose();
                server.Dispose();
            }

            server = new NamedPipeServerStream("BotPipe", PipeDirection.InOut);
            reader = new StreamReader(server, Encoding.UTF8, leaveOpen: true);
            writer = new StreamWriter(server, Encoding.UTF8, leaveOpen: true);
            Console.WriteLine("Waiting for admin console...");
            server.WaitForConnection();
            Console.WriteLine("AdminConsole connected...");
        }

        private static string ReadInput()
        {
            return reader.ReadLine() ?? throw new EndOfStreamException();
        }

        private static bool ReadConsoleCommands(CancellationTokenSource cts, DateTime startTime)
        {
            string? consoleCommand = null;
            while (!(consoleCommand == "/start" && cts.IsCancellationRequested))
            {
                consoleCommand = ReadInput();
                switch (consoleCommand)
                {
                    case "/help":
                        writer.WriteLine("/help; /clearlog; /start; /stop; /exit; /status; /reports; /cancelreport");
                        writer.Flush();
                        break;
                    case "/clearlog":
                        if (!Console.IsOutputRedirected)
                            Console.Clear();
                        writer.WriteLine("Log is cleared.");
                        writer.Flush();
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
                        try
                        {
                            writer.WriteLine("Bye.");
                            writer.Flush();
                        }
                        catch (IOException)
                        {

                        }
                        return false;

                    case "/status":
                        writer.WriteLine("Write userId:");
                        writer.Flush();
                        long userId;
                        if (long.TryParse(ReadInput(), out userId) && DatabaseManager.CheckUserExists(userId))
                        {
                            writer.WriteLine("Write status (Confirmed, Suspected, Banned):");
                            writer.Flush();
                            string? status = ReadInput();
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
                            if (long.TryParse(ReadInput(), out userId) && DatabaseManager.CheckReportExists(userId))
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
