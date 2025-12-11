using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Client.ClientHandlers;

[Command(Command.StartGame)]
public class StartGameCommandHandler : IClientCommandHandler
{
    public Task Invoke(Socket sender, Dictionary<string, string> parameters, CancellationToken ct = default)
    {
        ConsoleUi.Info("Starting game... Be ready! Type 's' to see text");
        return Task.CompletedTask;
    }
}