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
    RoomName = 0x11,
    CommandResponse = 0x06,
    StartGame = 0x53,
    SendAllProgresses = 0x3E,
}
