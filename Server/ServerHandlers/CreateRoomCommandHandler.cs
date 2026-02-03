using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Server.ServerHandlers;

[Command(Command.CreateRoom)]
public class CreateRoomCommandHandler : IServerCommandHandler
{
    public async Task Invoke(Socket sender, Dictionary<string, string> parameters, ConcurrentDictionary<string, Room> rooms, CancellationToken ct = default)
    {
        var roomName = parameters["room"];
        var playerName = parameters["nickname"];
        var maxPlayers = int.Parse(parameters["maxPlayers"]);
        var room = new Room(roomName, maxPlayers);
        
        Console.WriteLine($"Room {roomName} has been created");
        
        rooms.TryAdd(roomName, room);
        
        var player = new Player(sender, playerName);
        try
        {
            room.TryAddPlayer(player);
            Console.WriteLine($"Player {playerName} is added to room {roomName}");
            await sender.SendRoom(room);
        }
        catch (ArgumentException)
        {
            await sender.SendCommandResponse(CommandResponse.RoomIsFull);
        }
    }
}