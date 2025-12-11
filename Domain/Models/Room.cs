using System.Net.Sockets;

namespace Domain.Models;

public class Room
{
    // TODO: переделать этот кринж в чтение из файла с текстами
    private static readonly List<string> Textes = 
    [
        "Любовь это боль, которая выжигает все внутри, подобно огню, поглощающему все чувства, которые находятся в душе.",
        "Винить другого, как и себя - все равно, что хвалить себя же, это любимейшее занятие всех людей, тешить свое тщеславие, льстить себе же.",
        "Жизнь - это не рай, но ее необходимо изменять в лучшую сторону, а не в худшую, чтобы в конце пути не бояться оказаться в аду.",
        "Влюбленные бесстрашны и отважны, они, как птицы, пролетают самый яростный огонь даже не опалив крыльев.",
        "Мы несем в себе собственную истину, которая является комбинацией множества истин, заимствованных у других.",
        "Мы на многое не отваживаемся не потому что оно трудно, оно трудно именно потому, что мы на него не отваживаемся."
    ];
    public string Name { get; set; }
    public string Text { get; private set; }
    public int MaxPlayers { get; set; }
    public bool IsGameStarted { get; private set; } = false;
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
        if (_players.Count == MaxPlayers || IsGameStarted)
        {
            _players.Add(player);
        }
        
        throw new ArgumentException("room is full");
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