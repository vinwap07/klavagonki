using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Server.ServerHandlers;

[Command(Command.ReadyToStart)]
public class ReadyToStartCommandHandler : IServerCommandHandler
{
    public async Task Invoke(Socket sender, Dictionary<string, string> parameters, ConcurrentDictionary<string, Room> rooms, CancellationToken ct = default)
    {
        var roomName = parameters["room"];
        var playerName = parameters["nickname"];
        var room = rooms
            .Select(r => r)
            .First(r => r.Key == roomName);
        var player = room.Value.GetPlayer(playerName);
        player.IsReady = true;
        await sender.SendOk();
        try
        {
            room.Value.TryStartGame();
            Console.WriteLine("Game started");
            var players = room.Value.GetPlayers();
            foreach (var p in players)
            {
                await p.Connector.SendStartGame();
                await p.Connector.SendText(room.Value.Text);
            }
            
        }
        catch 
        {
            Console.WriteLine("Not all players are ready");
        }
    }
}