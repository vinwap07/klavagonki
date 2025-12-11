namespace Domain.Models;

public class GameResult
{
    public string PlayerName { get; set; }
    public int Place { get; set; }
    public int Errors { get; set; }
    public double CharPerMinute { get; set; }
    public double Accuracy { get; set; }
    public TimeSpan? TextingTime { get; set; }

    public GameResult(Player player)
    {
        PlayerName = player.Nickname;
        TextingTime = player.FinishTime - player.StartTime;
        Place = player.Place;
        Errors = player.ErrorsCount;
        CharPerMinute = (int)Math.Round(player.CurrentProgress / TextingTime.Value.TotalMinutes);
        Accuracy = Math.Round(1 - (double)Errors/player.CurrentProgress, 2);
    }

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        dict["PlayerName"] = PlayerName;
        dict["Place"] = Place.ToString();
        dict["Errors"] = Errors.ToString();
        dict["CharPerMinute"] = CharPerMinute.ToString();
        dict["Accuracy"] = Accuracy.ToString();
        dict["TextingTime"] = TextingTime.ToString();
        
        return dict;
    }
}