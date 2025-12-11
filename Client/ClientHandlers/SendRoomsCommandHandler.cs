using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Client.ClientHandlers;

[Command(Command.SendRooms)]
public class SendRoomsCommandHandler : IClientCommandHandler
{
    public Task Invoke(Socket sender, Dictionary<string, string> parameters, CancellationToken ct = default)
    {
        ConsoleUi.Info("Rooms:");
        foreach (var pair in parameters)
        {
            ConsoleUi.Info(pair.Key);
        }
        ConsoleUi.Info("To choose rome type 'join room', if you want create your room, type 'create room'");
        return Task.CompletedTask;
    }
}