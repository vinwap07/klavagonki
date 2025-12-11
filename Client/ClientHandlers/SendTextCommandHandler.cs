using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Client.ClientHandlers;

[Command(Command.SendText)]
public class SendTextCommandHandler : IClientCommandHandler
{
    public Task Invoke(Socket sender, Dictionary<string, string> parameters, CancellationToken ct = default)
    {
        var text = parameters["text"];
        ConsoleUi.Info("Text:");
        Console.WriteLine(text);
        ConsoleUi.Info("typing text faster!");
        return Task.CompletedTask;
    }
}