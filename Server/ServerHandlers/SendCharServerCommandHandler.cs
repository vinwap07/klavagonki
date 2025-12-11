using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Server.ServerHandlers;

[Command(Command.SendChar)]
public class SendCharServerCommandHandler : IServerCommandHandler
{
    public async Task Invoke(Socket sender, Dictionary<string, string> parameters, ConcurrentDictionary<string, Room> rooms, CancellationToken ct = default)
    {
        var roomName = parameters["room"];
        var c = parameters["c"];
        var playerName = parameters["nickname"];
        var charTime = TimeOnly.FromDateTime(DateTime.Now);
        var room = rooms
            .Select(r => r)
            .First(r => r.Key == roomName);
        var player = room.Value.GetPlayer(playerName);

        var isCorrect = c == room.Value.Text[player.CurrentProgress].ToString(); 
        if (isCorrect)
        {
            player.CurrentProgress += 1;
            if (player.CurrentProgress == room.Value.Text.Length)
            {
                await HandleFinishingPlayer(player, room.Value, charTime);
            }
        }
        else
        {
            player.ErrorsCount += 1;
        }

        await sender.SendCheckedChar(isCorrect);
    }

    private async Task HandleFinishingPlayer(Player player, Room room, TimeOnly finishTime)
    {
        var finishedPlayersCount = room
            .GetPlayers()
            .Select(p => p)
            .Count(p => p.IsFinished == true);
        player.IsFinished = true;
        player.Place = finishedPlayersCount + 1;
        player.FinishTime = finishTime;
        var results = new GameResult(player).ToDictionary();

        foreach (var p in room.GetPlayers())
        {
            await p.Connector.SendResult(results);
        }
    }
}