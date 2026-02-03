using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Client;

public class GameManager
{
    public Socket Socket { get; set; }
    private CancellationTokenSource _cts;
    private string _targetText;
    private string _nickname;
    private bool _isRunning = true;
    private int _selectedMenuIndex = 0;
    private List<string> _rooms;
    private Room _currentRoom;
    private int _selectedRoomIndex = 0;
    private bool _inRoomLobby = false;
    private bool _isReady = false;
    private int _playersCount = 0;

    public GameManager(string host, int port)
    {
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket.Connect(host, port);
        _cts = new CancellationTokenSource();
        _rooms = new List<string>();
    }

    public async Task StartGame()
    {
        await HandleResponse();
        Console.CursorVisible = false;
        Console.Clear();


        ConsoleUi.CenterWrite("Введите ваше имя:");
        Console.SetCursorPosition((Console.WindowWidth - 20) / 2, Console.CursorTop + 1);
        Console.CursorVisible = true;
        _nickname = Console.ReadLine()?.Trim() ?? "Игрок";
        Console.CursorVisible = false;

        while (_isRunning)
        {
            ShowMainMenu();
            await HandleMainMenuInput();
        }
    }
    private void ShowMainMenu()
    {
        Console.Clear();

        ConsoleUi.CenterWriteLineInfo("=== КЛАВОГОНКИ ===", 2);
        ConsoleUi.CenterWriteLine($"Игрок: {_nickname}", 1);
        Console.WriteLine();

        string[] menuItems = { "Создать комнату", "Список комнат", "Выйти" };

        for (int i = 0; i < menuItems.Length; i++)
        {
            int left = (Console.WindowWidth - 20) / 2;
            Console.SetCursorPosition(left, Console.CursorTop + 1);

            if (i == _selectedMenuIndex)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Cyan;
                Console.Write($"> {menuItems[i]}");
                Console.ResetColor();
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"  {menuItems[i]}");
            }
        }

        Console.WriteLine();
        ConsoleUi.CenterWriteLineInfo("Используйте ↑↓ для выбора, Enter для подтверждения", 2);
    }

    private async Task HandleMainMenuInput()
    {
        while (true)
        {
            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    _selectedMenuIndex = Math.Max(0, _selectedMenuIndex - 1);
                    ShowMainMenu();
                    break;
                case ConsoleKey.DownArrow:
                    _selectedMenuIndex = Math.Min(2, _selectedMenuIndex + 1);
                    ShowMainMenu();
                    break;
                case ConsoleKey.Enter:
                    await ExecuteMenuAction();
                    return;
                case ConsoleKey.Escape:
                    _isRunning = false;
                    return;
            }
        }
    }

    private async Task ExecuteMenuAction()
    {
        switch (_selectedMenuIndex)
        {
            case 0:
                await CreateRoom();
                break;
            case 1:
                await ShowRoomList();
                break;
            case 2:
                _isRunning = false;
                break;
        }
    }

    private async Task CreateRoom()
    {
        Console.Clear();
        ConsoleUi.CenterWriteLineInfo("=== СОЗДАНИЕ КОМНАТЫ ===", 2);

        Console.CursorVisible = true;

        ConsoleUi.CenterWrite("Название комнаты: ", 2);
        string roomName = Console.ReadLine()?.Trim();
        while (roomName.Length == 0)
        {
            ConsoleUi.CenterWrite("Введите название комнаты: ", 0);
            roomName = Console.ReadLine()?.Trim();
        }
        
        ConsoleUi.CenterWrite("Максимальное количество игроков: ", 1);
        int maxPlayers;
        while (!int.TryParse(Console.ReadLine(), out maxPlayers) || maxPlayers < 2 || maxPlayers > 10)
        {
            ConsoleUi.CenterWrite("Введите число от 2 до 10: ", 0);
        }

        await Socket.SendCreateRoom(_nickname, roomName, maxPlayers);
        var response = await HandleResponse();

        Console.CursorVisible = false;

        var room = new Room(roomName, maxPlayers);
        _playersCount = int.Parse(response.parameters["playersCount"]);
        _rooms.Add(room.Name);
        _currentRoom = room;
        var player = new Player();
        player.Nickname = _nickname;
        _currentRoom.TryAddPlayer(player);

        await ShowRoomScreen(_playersCount);
    }

    private async Task<(Command Command, Dictionary<string, string> parameters)> HandleResponse()
    {
        ArraySegment<byte> buffer = new byte[128];

        var messageBytesCount = await Socket.ReceiveAsync(buffer, SocketFlags.None);
        var message = buffer[..messageBytesCount];
        var response = PackageParser.TryParse(message, out var commandResponse);
        var parameters = PayloadSerializer.Decode(response.Value.Payload);
        CheckIsError(response, parameters);
        return (response.Value.Command, parameters);
    }

    private bool CheckIsError((Command Command, byte[] Payload)? response, Dictionary<string, string> parameters)
    {
        if (response.Value.Command == Command.CommandResponse && (CommandResponse)parameters["status"][0] != CommandResponse.OK)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleUi.CenterWriteLine("Ошибка на сервере");
            Console.ResetColor();
            ShowMainMenu();
            return true;
        } 
        
        return false;
    }

    private async Task ShowRoomList()
    {
        Console.Clear();
        ConsoleUi.CenterWriteLineInfo("=== СПИСОК КОМНАТ ===", 2);
        await Socket.SendGetRooms();
        var response = await HandleResponse();
        var rooms = response.parameters["rooms"];
        if (rooms == "[]")
        {
            ConsoleUi.CenterWriteLine("Комнат пока нет. Создайте первую!", 3);
            ConsoleUi.CenterWriteLineInfo("Нажмите любую клавишу для возврата...", 1);
            Console.ReadKey();
            return;
        }
        
        _rooms = rooms.Split(',').ToList();
        
        Console.WriteLine();

        for (int i = 0; i < _rooms.Count; i++)
        {
            int left = (Console.WindowWidth - 40) / 2;
            Console.SetCursorPosition(left, Console.CursorTop);

            string roomInfo = $"{i + 1}. {_rooms[i]}";

            if (i == _selectedRoomIndex)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write($"> {roomInfo}");
                Console.ResetColor();
            }
            else
            {
                Console.Write($"  {roomInfo}");
            }

            Console.WriteLine();
        }

        Console.WriteLine();
        ConsoleUi.CenterWriteLineInfo("↑↓ Выбрать комнату | Enter Войти | ESC Выйти", 2);

        await HandleRoomListInput();
    }

    private async Task HandleRoomListInput()
    {
        while (true)
        {
            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    _selectedRoomIndex = Math.Max(0, _selectedRoomIndex - 1);
                    await ShowRoomList();
                    break;
                case ConsoleKey.DownArrow:
                    _selectedRoomIndex = Math.Min(_rooms.Count - 1, _selectedRoomIndex + 1);
                    await ShowRoomList();
                    break;
                case ConsoleKey.Enter:
                    await JoinRoom(_selectedRoomIndex);
                    break;
                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    private async Task JoinRoom(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= _rooms.Count)
        {
            return;
        }
        
        await Socket.SendJoinRoom(_rooms[roomIndex], _nickname);
        var response = await HandleResponse();
        try
        {
            var roomName = response.parameters["room"];
            var maxPlayers = int.Parse(response.parameters["maxPlayers"]);
            var _playersCount = int.Parse(response.parameters["playersCount"]);
            _currentRoom = new Room(roomName, maxPlayers);
            if (!_currentRoom.GetPlayers().Exists(p => p.Nickname == _nickname))
            {
                var player = new Player();
                player.Nickname = _nickname;
                _currentRoom.GetPlayers().Add(player);
            }

            await ShowRoomScreen(_playersCount);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleUi.CenterWriteLine($"Error: {ex.Message}");
        }
    }
    private async Task ShowRoomScreen(int playersCount)
{
    _inRoomLobby = true;
    var cts = new CancellationTokenSource();
    object consoleLock = new object();

    // Запускаем фоновое обновление с токеном отмены
    var updateTask = Task.Run(async () =>
    {
        while (_inRoomLobby && !cts.Token.IsCancellationRequested)
        {
            try
            {
                lock (consoleLock)
                {
                    Console.Clear();
                    DisplayRoom(_playersCount);
                }
                
                await Socket.SendGetRoomInfo(_currentRoom.Name);
                var response = await HandleResponse();
                if (response.Command == Command.SendRoom)
                {
                    _playersCount = int.Parse(response.parameters["playersCount"]);
                }
                
                await Task.Delay(1000, cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Нормальный выход при отмене
                break;
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                {
                    Console.Clear();
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
                await Task.Delay(1000);
            }
        }
    }, cts.Token);

    while (_inRoomLobby)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true).Key;
            
            if (key == ConsoleKey.Enter)
            {
                // 1. Отменяем фоновое обновление
                cts.Cancel();
                
                // 2. Очищаем экран один раз
                lock (consoleLock)
                {
                    Console.Clear();
                    _isReady = true;
                    DisplayRoom(_playersCount);
                }
                
                // 3. Ждем завершения фоновой задачи
                await updateTask;
                
                // 4. Отправляем готовность и ждем начала игры
                await HandleReadyToStart();
                
                var progresses = new Dictionary<string, string>
                {
                    {_nickname, "0"}
                };
                
                // 5. Запускаем игру
                await StartTypingGame(progresses);
                break;
            }
            else if (key == ConsoleKey.Escape)
            {
                cts.Cancel();
                await updateTask;
                
                await Socket.SendLeaveRoom(_currentRoom.Name, _nickname);
                var response = await HandleResponse();
                if (response.Command == Command.CommandResponse)
                {
                    _inRoomLobby = false;
                }
                break;
            }
        }
        
        // Небольшая задержка для снижения нагрузки
        await Task.Delay(100);
    }
}

    private async Task HandleReadyToStart()
    {
        await Socket.SendReadyToStart(_currentRoom.Name, _nickname);
        _isReady = true;
    
        // Отображаем сообщение о готовности
        Console.Clear();
        ConsoleUi.CenterWriteLineInfo($"=== {_currentRoom.Name.ToUpper()} ===", 2);
        ConsoleUi.CenterWriteLine($"Игроков: {_playersCount}/{_currentRoom.MaxPlayers}", 1);
        Console.WriteLine();
        ConsoleUi.CenterWriteLineInfo("Вы готовы! Ждем готовность остальных игроков...", 2);
    
        // Ждем команду StartGame от сервера
        while (true)
        {
            var response = await HandleResponse();
            if (response.Command == Command.StartGame)
            {
                _targetText = response.parameters["text"];
                break;
            }
            
            if (response.Command == Command.SendRoom)
            {
                _playersCount = int.Parse(response.parameters["playersCount"]);
            }
            // Обновляем отображение
            Console.Clear();
            ConsoleUi.CenterWriteLineInfo($"=== {_currentRoom.Name.ToUpper()} ===", 2);
            ConsoleUi.CenterWriteLine($"Игроков: {_playersCount}/{_currentRoom.MaxPlayers}", 1);
            Console.WriteLine();
            ConsoleUi.CenterWriteLineInfo("Вы готовы! Ждем готовность остальных игроков...", 2);
            ConsoleUi.CenterWriteLine($"Ожидание... ({_playersCount}/{_currentRoom.MaxPlayers} готовы)", 1);
        }
    }
    private void DisplayRoom(int playersCount)
    {
        ConsoleUi.CenterWriteLineInfo($"=== {_currentRoom.Name.ToUpper()} ===", 2);
        ConsoleUi.CenterWriteLine($"Игроков: {playersCount}/{_currentRoom.MaxPlayers}", 1);
        Console.WriteLine();

        var players = _currentRoom.GetPlayers();
        
        Console.WriteLine();
        
        if (_isReady)
        {
            ConsoleUi.CenterWriteLineInfo("Вы готовы! Ждем готовность остальных игроков");
        }
        else
        {
            ConsoleUi.CenterWriteLineInfo("Нажмите Enter чтобы начать гонку", 2);

            ConsoleUi.CenterWriteLineInfo("Нажмите Escape чтобы выйти из комнаты", 1);    
        }
    }

    private async Task StartTypingGame(Dictionary<string, string> progresses)
    {
        Console.Clear();
        ConsoleUi.CenterWriteLineInfo("=== ГОНКА НАЧАЛАСЬ! ===", 2);
        ConsoleUi.CenterWriteLineInfo("Набирайте текст как можно быстрее!", 1);
        Console.WriteLine();
        Console.WriteLine();
    
        int currentCharIndex = 0;
        int errors = 0;
        DateTime startTime = DateTime.Now;
        
        ConsoleUi.CenterWrite("Вводите: ", 1);
        int inputLeft = Console.CursorLeft;
        int inputTop = Console.CursorTop;
    
        Console.WriteLine();
        foreach (var progress in progresses)
        {
            ConsoleUi.CenterWrite($"{progress.Key} - {(int.Parse(progress.Value) / _targetText.Length) * 100}%");
        }
        
        foreach (var player in _currentRoom.GetPlayers())
        {
            player.StartTime = TimeOnly.FromDateTime(DateTime.Now);
        }
    
        while (currentCharIndex < _targetText.Length)
        {
            
            // Обновляем прогресс текущего игрока
            var currentPlayer = _currentRoom.GetPlayers().Find(p => p.Nickname == _nickname);
            if (currentPlayer != null)
            {
                currentPlayer.CurrentProgress = (currentCharIndex / _targetText.Length) * 100;
            }
    
            Console.SetCursorPosition(inputLeft, inputTop);
    
            // Очищаем строку ввода
            Console.Write(new string(' ', Console.WindowWidth - inputLeft - 2));
            Console.SetCursorPosition(inputLeft, inputTop);
    
            // Показываем уже введенные символы с цветовой индикацией
            for (int i = 0; i < currentCharIndex; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(_targetText[i]);
            }
    
            Console.ResetColor();
    
            // Показываем оставшийся текст
            for (int i = currentCharIndex; i < _targetText.Length; i++)
            {
                Console.Write(_targetText[i]);
            }
            
            Console.WriteLine();
    
            Console.SetCursorPosition(inputLeft + currentCharIndex, inputTop);
    
            var key = Console.ReadKey(true);
            await Socket.SendChar(key.KeyChar, _currentRoom.Name, _nickname);
            var charResponse = await HandleResponse();
            var parameters = charResponse.parameters;
            var isCorrect = parameters["isCorrect"];
            parameters.Remove("isCorrect");
            progresses = parameters;
            if (isCorrect == "True")
            {
                currentCharIndex++;
                DisplayProgressBar(progresses, _targetText.Length);
            }
            else
            {
                errors++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(key.KeyChar);
                Console.ResetColor();
                DisplayProgressBar(progresses, _targetText.Length);
                Thread.Sleep(50);
                Console.SetCursorPosition(inputLeft + currentCharIndex, inputTop);
            }
        }
    
        var response = await HandleResponse();
        var place = int.Parse(response.parameters["Place"]);
        errors = int.Parse(response.parameters["Errors"]);
        var charPerMinute = response.parameters["CharPerMinute"];
        var accuracy = response.parameters["Accuracy"];
        var textingTime = response.parameters["TextingTime"];
        
        _currentRoom.IsGameStarted = false;
        _targetText = string.Empty;
        
        var finishedPlayer = _currentRoom.GetPlayers().Find(p => p.Nickname == _nickname);
        if (finishedPlayer != null)
        {
            finishedPlayer.CurrentProgress = 0;
            finishedPlayer.IsFinished = false;
            finishedPlayer.IsReady = false;
            
        }
    
        await ShowGameStats(textingTime, errors, charPerMinute, accuracy, place);
    }
    
    
    private void DisplayProgressBar(Dictionary<string, string> progresses, int textLength)
    {
        Console.WriteLine();
        ConsoleUi.CenterWriteLineInfo("=== ПРОГРЕСС ИГРОКОВ ===");
    
        foreach (var progress in progresses)
        {
            string playerName = progress.Key;
            if (progress.Key == _nickname)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                playerName = $"> {playerName} <";
            }
        
            Console.Write($"{playerName,-15}: ");
        
            // Прогресс-бар
            int barWidth = 30;
            int filledWidth = (int)(barWidth * double.Parse(progress.Value)/textLength);

            Console.Write("[");
            Console.Write(new string('█', filledWidth));
            Console.Write(new string(' ', barWidth - filledWidth));
            Console.Write($"] {Math.Round(100 * double.Parse(progress.Value) / textLength, 2)}%");
            Console.WriteLine();
            Console.ResetColor();
        }
    }


    private async Task ShowGameStats(string time, int errors, string speed, string accuracy, int place)
    {
        Console.Clear();
        ConsoleUi.CenterWriteLineInfo("=== РЕЗУЛЬТАТЫ ===", 3);
        Console.WriteLine();

        ConsoleUi.CenterWriteLine($"Время: {time} секунд", 1);
        ConsoleUi.CenterWriteLine($"Ошибок: {errors}", 1);
        ConsoleUi.CenterWriteLine($"Скорость: {speed} символов/минуту", 1);
        ConsoleUi.CenterWriteLine($"Точность: {accuracy}%", 1);
        ConsoleUi.CenterWriteLine($"Занятое место: {place}", 1);

        Console.WriteLine();
        ConsoleUi.CenterWriteLineInfo("Нажмите любую клавишу чтобы вернуться в меню...", 2);
        Console.ReadKey();
        await Socket.SendLeaveRoom(_currentRoom.Name, _nickname);
        await HandleResponse();
        _isReady = false;
        _currentRoom.Name = string.Empty;
        _currentRoom.MaxPlayers = 0;
    }
}