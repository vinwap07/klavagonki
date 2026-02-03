using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Server.ServerHandlers;

[Command(Command.GetRooms)]
public class GetRoomsCommandHandler : IServerCommandHandler
{
    public async Task Invoke(Socket sender, Dictionary<string, string> parameters, ConcurrentDictionary<string, Room> rooms, CancellationToken ct = default)
    {
        var roomsArray = rooms.Values.ToArray();
        await sender.SendRooms(roomsArray);
    }
}