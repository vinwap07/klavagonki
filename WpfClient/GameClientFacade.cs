using System.Net.Sockets;
using Domain;

namespace WpfClient;

public class GameClientFacade
{
    private readonly KlavagonkiClient _client;
    private readonly Socket _socket;
    private bool _isDisposed;

    public event Action<Dictionary<string, string>>? OnGameTextReceived; // Получен текст для гонки
    public event Action<Dictionary<string, string>>? OnRoomJoined; // Успешно присоединились к комнате
    public event Action<Dictionary<string, string>>? OnRoomsListReceived; // Получен список комнат
    public event Action<Dictionary<string, string>>? OnGameFinished; // Игра завершена
    public event Action<Dictionary<string, string>>? OnProgressUpdated; // Обновлен прогресс игроков
    public event Action<string>? OnError; // Ошибка
    public event Action<Dictionary<string, string>> OnGameStarted; // Игра началась
    public event Action? OnConnectedToServer; // Подключились к серверу
    public event Action? OnDisconnected; // Отключились

    public bool IsConnected => _client.IsConnected;
    public Guid CurrentRoomId { get; private set; } = Guid.Empty;
    public string Nickname { get; private set; } = string.Empty;

    public GameClientFacade(string serverIp = "127.0.0.1", int serverPort = 8888)
    {
        _client = new KlavagonkiClient(serverIp, serverPort);
        _client.OnDataReceived += HandleDataReceived;
        _client.OnConnectionError += (ex) => OnError?.Invoke($"Connection error: {ex.Message}");
        _client.OnDisconnected += () => OnDisconnected?.Invoke();
        _socket = _client.Socket;
    }

    public async Task ConnectAsync()
    {
        try
        {
            await _client.ConnectAsync();
            OnConnectedToServer?.Invoke();
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to connect: {ex.Message}");
            throw;
        }
    }
    
    public async Task CreateRoomAsync(string nickname, string roomName, int maxPlayers)
    {
        Nickname = nickname;
        await _socket.SendCreateRoom(nickname, roomName, maxPlayers);
    }

    public async Task JoinRoomAsync(string roomId, string nickname)
    {
        Guid.TryParse(roomId, out var guid);
        await _socket.SendJoinRoom(guid, nickname);
    }


    public async Task LeaveRoomAsync()
    {
        if (CurrentRoomId != Guid.Empty)
        {
            await _socket.SendLeaveRoom(CurrentRoomId, Nickname);
            CurrentRoomId = Guid.Empty;
        }
    }
    
    public async Task GetRoomsAsync()
    {
        await _socket.SendGetRooms();
    }
    
    public async Task SendCharAsync(char character)
    {
        await _socket.SendChar(character, CurrentRoomId, Nickname);
    }
    
    public async Task SendReadyAsync()
    {
        await _socket.SendReadyToStart(CurrentRoomId, Nickname);
    }

    private void HandleDataReceived(byte[] data)
    {
        var response = PackageParser.TryParse(data, out var commandResponse);
        var command = response.Value.Command;
        var parameters = PayloadSerializer.Decode(response.Value.Payload);
        ProcessCommand(command, parameters);
    }

    private void ProcessCommand(Command command, Dictionary<string, string> parameters)
    {
        
        switch (command)
        {
            case Command.RoomId:
                OnRoomJoined?.Invoke(parameters);
                break;

            case Command.SendText:
                OnGameTextReceived?.Invoke(parameters);
                break;

            case Command.SendRooms:
                OnRoomsListReceived?.Invoke(parameters);
                break;

            case Command.StartGame:
                OnGameStarted?.Invoke(parameters);
                break;

            case Command.SendAllProgresses:
                OnProgressUpdated?.Invoke(parameters);
                break;

            case Command.Result:
                OnGameFinished?.Invoke(parameters);
                break;

            case Command.CommandResponse:
                var response = parameters["status"];
                if (response != ((byte)CommandResponse.OK).ToString())
                {
                    OnError?.Invoke($"Server error: {response}");
                }

                break;
        }
    }

    public void Disconnect()
    {
        _client.Disconnect();
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
