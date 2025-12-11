using Domain;
using Domain.Models;
using System.Net.Sockets;

namespace Client;

public class KlavagonkiClient
{
    private readonly Socket _socket;
    private readonly Player _player = new Player();
    private string _roomName;
    private CancellationTokenSource _cts;

    public KlavagonkiClient(string host, int port)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(host, port);
    }

    public async Task Start()
    {
        await HandleResponse();
        ConsoleUi.Info("You connected to Klavagonki.");
        var nickname = ConsoleUi.Input("Please, enter your name:");
        _player.Nickname = nickname;
        
        ConsoleUi.Info("You can create your own room or connect to exists one. Type 'create room' or 'get rooms'");
        while (true)
        {
            var command = ConsoleUi.Input("type here:");
            await HandleCommand(command);
            await HandleResponse();
        }
    }

    private async Task HandleCommand(string command)
    {
        switch (command)
        {
            case "create room":
                await HandleCreateRoom();
                break;
            case "get rooms":
                await HandleGetRooms();
                break;
            case "leave room":
                await HandleLeaveRoom();
                break;
            case "ready to start":
                await HandleReadyToStart();
                break;
            case "join room":
                await HandleJoinRoom();
                break;
            case "s":
                await HandleResponse();
                break;
            default:
                await HandleChar(command);
                break;
        }
    }

    private async Task HandleChar(string ch)
    {
        await _socket.SendChar(ch[0], _roomName, _player.Nickname);
    }

    private async Task HandleCreateRoom()
    {
        _roomName = ConsoleUi.Input("Enter room name:");
        var isParsing = int.TryParse(ConsoleUi.Input("Enter max players:"), out var maxPlayers);
        if (!isParsing)
        {
            maxPlayers = 2;
        }
        await _socket.SendCreateRoom(_player.Nickname, _roomName, maxPlayers);
    }

    private async Task HandleGetRooms()
    {
        await _socket.SendGetRooms();
    }

    private async Task HandleLeaveRoom()
    {
        await _socket.SendLeaveRoom(_roomName, _player.Nickname);
        _roomName = string.Empty;
    }

    private async Task HandleReadyToStart()
    {
        await _socket.SendReadyToStart(_roomName, _player.Nickname);
    }

    private async Task HandleJoinRoom()
    {
        _roomName = ConsoleUi.Input("Enter room name:");
        await _socket.SendJoinRoom(_roomName, _player.Nickname);
    }

    private async Task HandleResponse()
    {
        ArraySegment<byte> buffer = new byte[1024];

        var messageBytesCount = await _socket.ReceiveAsync(buffer, SocketFlags.None);
        var message = buffer[..messageBytesCount];
        var response = PackageParser.TryParse(message, out var commandResponse);

        await HandleClient(_socket, response.Value.Command, response.Value.Payload, _cts);
    }

    private async Task HandleClient(Socket connection, Command command, byte[] payload, CancellationTokenSource cts)
    {
        var commandHandler = ClientCommandHandlerFactory.GetHandler(command);
        var parameters = PayloadSerializer.Decode(payload);
        await commandHandler.Invoke(connection, parameters, cts.Token);
    }
}