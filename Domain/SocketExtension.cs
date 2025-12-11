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

    public static async Task SendRoomName(this Socket socket, string room)
    {
        var payload = PayloadSerializer.Encode(
            new Dictionary<string, string>
            {
                { "room", room }
            });
        var package = new PackageBuilder(payload, Command.RoomName).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendStartGame(this Socket socket)
    {
        var package = new PackageBuilder([], Command.StartGame).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendCreateRoom(this Socket socket, string nickname, string roomName, int maxPlayers)
    {
        var payload = PayloadSerializer.Encode(new Dictionary<string, string>
        {
            { "roomName", roomName },
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

    public static async Task SendCheckedChar(this Socket socket, bool isCorrect)
    {
        var payload = PayloadSerializer.Encode(
            new Dictionary<string, string>
            {
                { "isCorrect", isCorrect.ToString() }
            });
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
        var payload = Encoding.ASCII.GetBytes(string.Join(",", rooms.Select(r => r.Name)));
        var package = new PackageBuilder(payload, Command.SendRooms).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }

    public static async Task SendGetRooms(this Socket socket)
    {
        var package = new PackageBuilder([], Command.GetRooms).Build();
        await socket.SendAsync(package, SocketFlags.None);
    }
}
