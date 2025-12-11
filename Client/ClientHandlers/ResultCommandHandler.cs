using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Client.ClientHandlers;

[Command(Command.Result)]
public class ResultCommandHandler : IClientCommandHandler
{
    public Task Invoke(Socket sender, Dictionary<string, string> parameters, CancellationToken ct = default)
    {
        foreach (var pair in parameters)
        {
            ConsoleUi.Info($"{pair.Key}: {pair.Value}");
        }
        
        ConsoleUi.Info("You can create your own room or connect to exists one. Type 'create room' or 'get rooms'");
        return Task.CompletedTask;
    }
}