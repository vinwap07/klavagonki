using System.Net.Sockets;
using System.Text;
using Domain.Models;

namespace Domain;

public static class SocketExtension
{
    public static async Task SendOk(this Socket socket)
    {
        await socket.SendCommandResponse(CommandResponse.OK);
    }
    
    public static async Task SendCommandResponse(this Socket socket, CommandResponse commandResponse)
    {
        var payload = PayloadSerializer.Encode(
            new Dictionary<string, string>
            {
                { "status", ((byte)commandResponse).ToString() }
            });
        var package = new PackageBuilder(payload, Command.CommandResponse).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendReadyToStart(this Socket socket, string room, string nickname)
    {
        var payload = PayloadSerializer.Encode(
            new Dictionary<string, string>
            {
                { "room", room },
                { "nickname", nickname }
            });
        var package = new PackageBuilder(payload, Command.ReadyToStart).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendRoom(this Socket socket, Room room)
    {
        var payload = PayloadSerializer.Encode(
            new Dictionary<string, string>
            {
                { "room", room.Name },
                { "playersCount", room.GetPlayers().Count.ToString()},
                { "maxPlayers", room.MaxPlayers.ToString()}
            });
        var package = new PackageBuilder(payload, Command.SendRoom).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendStartGame(this Socket socket, string text)
    {
        var payload = PayloadSerializer.Encode(
            new Dictionary<string, string>
            {
                { "text", text }
            });
        var package = new PackageBuilder(payload, Command.StartGame).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendCreateRoom(this Socket socket, string nickname, string roomName, int maxPlayers)
    {
        var payload = PayloadSerializer.Encode(new Dictionary<string, string>
        {
            { "room", roomName },
            { "maxPlayers", maxPlayers.ToString() },
            { "nickname", nickname }
        });
        var package = new PackageBuilder(payload, Command.CreateRoom).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendJoinRoom(this Socket socket, string room, string nickname)
    {
        var payload = PayloadSerializer.Encode(
            new Dictionary<string, string>
            {
                { "room", room },
                { "nickname", nickname }
            });
        var package = new PackageBuilder(payload, Command.JoinRoom).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendLeaveRoom(this Socket socket, string room, string nickname)
    {
        var payload = PayloadSerializer.Encode(
            new Dictionary<string, string>
            {
                { "room", room },
                { "nickname", nickname }
            });
        var package = new PackageBuilder(payload, Command.LeaveRoom).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendGetRoomInfo(this Socket socket, string roomName)
    {
        var payload = PayloadSerializer.Encode(
            new Dictionary<string, string>
            {
                { "roomName", roomName }
            });
        var package = new PackageBuilder(payload, Command.GetRoomInfo).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendChar(this Socket socket, char c, string room, string nickname)
    {
        var payload = PayloadSerializer.Encode(
            new Dictionary<string, string>
            {
                { "room", room },
                { "c", c.ToString() },
                { "nickname", nickname }
            });
        var package = new PackageBuilder(payload, Command.SendChar).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendCheckedCharAndProgress(this Socket socket, bool isCorrect, Dictionary<string, string> progresses)
    {
        progresses.Add("isCorrect", isCorrect.ToString());
        var payload = PayloadSerializer.Encode(progresses);
        var package = new PackageBuilder(payload, Command.CheckChar).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendText(this Socket socket, string text)
    {
        var payload = PayloadSerializer.Encode(
        new Dictionary<string, string>
        {
            { "text", text }
        });
        var package = new PackageBuilder(payload, Command.SendText).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendResult(this Socket socket, Dictionary<string, string> results)
    {
        var payload = PayloadSerializer.Encode(results);
        var package = new PackageBuilder(payload, Command.Result).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendAllProgresses(this Socket socket, Dictionary<string, string> results)
    {
        var payload = PayloadSerializer.Encode(results);
        var package = new PackageBuilder(payload, Command.SendAllProgresses).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendRooms(this Socket socket, Room[] rooms)
    {
        var dict = new Dictionary<string, string>(); 
        if (rooms == null || rooms.Length == 0)
        {
            dict["rooms"] = "[]";
        }
        else
        {
            dict["rooms"] = string.Join(",", rooms.Select(r => r.Name));
        }
        var payload = PayloadSerializer.Encode(dict);
        var package = new PackageBuilder(payload, Command.SendRooms).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendGetRooms(this Socket socket)
    {
        var package = new PackageBuilder([], Command.GetRooms).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }
}
