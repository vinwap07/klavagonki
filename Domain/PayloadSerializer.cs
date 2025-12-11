using System.Text;

public class PayloadSerializer
{
    public static byte[] Encode(Dictionary<string, string> data)
    {
        string payload = string.Join(";", data.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        byte[] bytes = Encoding.ASCII.GetBytes(payload);
        return bytes;
    }

    public static Dictionary<string, string> Decode(byte[] payload)
    {
        if (payload.Length == 0)
        {
            return new Dictionary<string, string>();
        }
        
        string text = Encoding.ASCII.GetString(payload);
        var parameters = new Dictionary<string, string>();

        foreach (var pair in text.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2)
            {
                parameters[parts[0]] = parts[1];
            }
        }
        
        return parameters;
    }
}