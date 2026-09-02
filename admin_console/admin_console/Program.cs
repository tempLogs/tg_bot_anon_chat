using System;
using System.ComponentModel.Design;
using System.IO.Pipes;
using System.Reflection;
using System.Text;

namespace admin_console;

class Program
{
    private static readonly string specialChar = "h34qfdi62d3k";

    static async Task Main()
    {
        using var client = new NamedPipeClientStream(".", "BotPipe", PipeDirection.InOut);
        await client.ConnectAsync();

        Console.WriteLine("Connected...");
        Console.WriteLine("Bot response: Bot is active...");
        Console.WriteLine("To see the list of commands, enter \"/help\"");

        using var reader = new StreamReader(client);
        using var writer = new StreamWriter(client);

        while (true)
        {
            string? command = Console.ReadLine();

            writer.WriteLine(command);
            writer.Flush();
            string? response = reader.ReadLine();
            response ??= "null";

            while (response.Contains(specialChar))
            {
                response = response[..response.IndexOf(specialChar)] + "\n" + response[(response.IndexOf(specialChar) + 12)..];
            }

            Console.WriteLine($"Bot response: {response}");
            if (command == "/exit") break;
        }
    }
}