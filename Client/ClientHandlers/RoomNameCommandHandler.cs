using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Client.ClientHandlers;

[Command(Command.RoomName)]

public class RoomNameCommandHandler : IClientCommandHandler
{
    public Task Invoke(Socket sender, Dictionary<string, string> parameters, CancellationToken ct = default)
    {
        var roomName = parameters["room"];
        ConsoleUi.Success($"Room {roomName} is created");
        ConsoleUi.Info($"Enter 'ready to start' if you want start game or wait for another players added");
        return Task.CompletedTask;
    }
}