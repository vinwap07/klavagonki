using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Server.ServerHandlers;

[Command(Command.LeaveRoom)]
public class LeaveRoomCommandHandler : IServerCommandHandler
{
    public async Task Invoke(Socket sender, Dictionary<string, string> parameters, ConcurrentDictionary<string, Room> rooms, CancellationToken ct = default)
    {
        var roomName = parameters["room"];
        var playerName = parameters["nickname"];
        var room = rooms
            .Select(r => r)
            .First(r => r.Key == roomName);
        
        room.Value.DeletePlayer(playerName);
        Console.WriteLine($"{playerName} is now leaving room {room.Value.Name}");
        await sender.SendOk();
    }
}