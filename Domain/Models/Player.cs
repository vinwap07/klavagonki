using System.Net.Sockets;

namespace Domain.Models;

public class Player
{
    public Socket Connector { get; set; }
    public int Place { get; set; }
    public bool IsReady { get; set; }
    public string Nickname { get; set; } 
    public int CurrentProgress { get; set; } 
    public int ErrorsCount { get; set; }
    public bool IsFinished { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly FinishTime { get; set; }

    public Player(Socket connector, string nickname)
    {
        Connector = connector;
        Nickname = nickname;
    }

    public Player()
    {
    }
}