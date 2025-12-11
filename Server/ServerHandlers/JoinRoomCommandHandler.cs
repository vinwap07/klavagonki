using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Server.ServerHandlers;

[Command(Command.JoinRoom)]
public class JoinRoomCommandHandler : IServerCommandHandler
{
    public async Task Invoke(Socket sender, Dictionary<string, string> parameters, ConcurrentDictionary<string, Room> rooms, CancellationToken ct = default)
    {
        var roomName = parameters["room"];
        var playerName = parameters["nickname"];
        
        var room = rooms
            .Select(r => r)
            .First(r => r.Key == roomName);
        
        var player = new Player(sender, playerName);
        try
        {
            room.Value.TryAddPlayer(player);
            Console.WriteLine($"Player {playerName} is added to room {roomName}");
            await sender.SendOk();
        }
        catch (ArgumentException)
        {
            await sender.SendCommandResponse(CommandResponse.RoomIsFull);
        }
    }
}