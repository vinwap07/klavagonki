using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Server.ServerHandlers;

[Command(Command.GetRoomInfo)]
public class GetRoomInfoCommandHandler : IServerCommandHandler
{
    public async Task Invoke(Socket sender, Dictionary<string, string> parameters, ConcurrentDictionary<string, Room> rooms, CancellationToken ct = default)
    {
        var roomName = parameters["roomName"];
        var room = rooms[roomName];
        await sender.SendRoom(room);
    }
}