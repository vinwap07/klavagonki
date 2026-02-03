using System.Net.Sockets;

namespace Domain.Models;

public class Room
{
    // TODO: переделать этот кринж в чтение из файла с текстами
    private static readonly List<string> Textes = 
    [
        "My family is very important to me. ",
        "My sister likes to cook with my grandmother.",
        "She came from Italy when I was two years old.",
        "My sister is a nervous girl, but she is very kind.",
        "I have a younger brother. He just started high school.",
        "Sometimes they visit me in New York. I am happy."
    ];
    public string Name { get; set; }
    public string Text { get; private set; }
    public int MaxPlayers { get; set; }
    public bool IsGameStarted { get; set; } = false;
    private List<Player> _players = new List<Player>();

    public Room(string name, int maxPlayers)
    {
        if (maxPlayers < 2)
        {
            throw new ArgumentException("maxPlayers must be greater than 2");
        }
        Name = name;
        MaxPlayers = maxPlayers;
        var random = new Random();
        Text = Textes[random.Next(Textes.Count)];
    }

    public void TryAddPlayer(Player player)
    {
        if (_players.Count == MaxPlayers)
        {
            throw new ArgumentException("room is full");
        }
        if (IsGameStarted)
        {
            throw new ArgumentException("game is already started");
        }

        _players.Add(player);
    }

    public void DeletePlayer(string playerName)
    {
        var player = _players
            .Select(p => p)
            .First(p => p.Nickname == playerName);
        _players.Remove(player);
    }

    public void TryStartGame()
    {
        if (_players.All(p => p.IsReady))
        {
            IsGameStarted = true;   
            var startTime = TimeOnly.FromDateTime(DateTime.Now);
            foreach (var player in _players)
            {
                player.StartTime = startTime;
            }
            
            return;
        }
        
        throw new ArgumentException("not all players are ready");
    }

    public Player GetPlayer(string playerName)
    {
        return _players.FirstOrDefault(p => p.Nickname == playerName);
    }

    public List<Player> GetPlayers()
    {
        return _players;
    }
}