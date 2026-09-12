using System.IO.Pipes;

namespace admin_console;

class Program
{
    private static readonly string specialChar = "h34qfdi62d3k";

    static async Task Main()
    {
        try
        {
            await RunConsole();
        }
        catch (IOException)
        {
            Console.WriteLine("Connection to the bot was closed. Check the bot window for errors.");
        }
    }

    private static async Task RunConsole()
    {
        using var client = new NamedPipeClientStream(".", "BotPipe", PipeDirection.InOut);
        await client.ConnectAsync();

        Console.WriteLine("Connected...");
        Console.WriteLine("Bot response: Bot is active...");
        Console.WriteLine("To see the list of commands, enter \"/help\"");

        using var reader = new StreamReader(client, leaveOpen: true);
        using var writer = new StreamWriter(client, leaveOpen: true);

        while (true)
        {
            string? command = Console.ReadLine();
            if (command == null) break;

            writer.WriteLine(command);
            writer.Flush();
            string? response = reader.ReadLine();
            if (response == null)
            {
                Console.WriteLine("Connection to the bot was closed. Check the bot window for errors.");
                break;
            }

            while (response.Contains(specialChar))
            {
                response = response[..response.IndexOf(specialChar)] + "\n" + response[(response.IndexOf(specialChar) + 12)..];
            }

            Console.WriteLine($"Bot response: {response}");
            if (command == "/exit") break;
        }
    }
}
