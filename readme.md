# 🏁 Klavagonki - Multiplayer Typing Speed Game

A console-based multiplayer typing competition game using the TCP protocol. Reliazed on C#, client-server app.

- TCP-based
- Real-time statistics 
- Private rooms

### Technologyes stack 

- .NET 8 
- System.Net.Sockets

### Quick start

1. Clone repository 

```
git clone https://github.com/vinwap07/klavagonki
cd klavagonki
```

2. Run server
```
dotnet run --project .\Server\Server.csproj
```

3. Run clients (from 2 to 10)
```
dotnet run --project .\Client\Client.csproj
```

## Data transfer
### Commans
``` csharp
public enum Command : byte
{
    // client -> server
    CreateRoom = 0x43,
    JoinRoom = 0x4A,
    LeaveRoom = 0x4C,
    ReadyToStart = 0x52,
    SendChar = 0x5F,
    GetRooms = 0x7B,
    
    // server -> client
    SendRooms = 0x7D,
    SendText = 0x22,
    CheckChar = 0x3F,
    Result = 0x3D,
    RoomId = 0x11,
    CommandResponse = 0x06,
    StartGame = 0x53,
    SendAllProgresses = 0x3E,
}
```
### Codes
``` csharp 
public enum CommandResponse
{
    OK,
    PlayerNotJoined,
    IncorrectSender,
    CommandIncorrect,
    PackageIncorrect,
    SessionNotFound,
    RoomIsFull,
}
```


**Message format:**
```
[package start: 1 byte][command: 1 byte][length: 2 bytes][data: N bytes][package end: 1 byte]
```


### Application structure

Solution consists of 3 projectes: 
- `Domain` provides tools for using the data transfer protocol
- `Server` receives and sends requests
- `Client` implements the user-visible portion of the application, sending and receiving requests

### Processing requests
To get the command and data from the received package, the PackageParser class with the TryParse method will help. It accepts an array of bytes - the received package. 

After that, through reflection, CommandHandlerFactory calls Invoke() for a specific Handler by its attribute, the command it processes. That is, each command has its own handler.

The request is processed inside the handler, and business logic is executed. If it is a server handler, a response is sent. If the handler is a client, then the UI is updated.


