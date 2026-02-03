using Domain;

public static class PackageParser
{
    public static (Command Command, byte[] Payload)? TryParse(ReadOnlySpan<byte> data, out CommandResponse commandResponse)
    {
        if (data.Length < 4 || data[0] != PackageMeta.Start || data[^1] != PackageMeta.End)
        {
            commandResponse = CommandResponse.PackageIncorrect;
            return null;
        }

        var length = 0;
        var hasCommand = false;
        
        foreach (var en in Enum.GetValues<Command>().Select(en => (byte)en))
        {
            if (en != data[PackageMeta.CommandByteIndex]) continue;
            length = data[PackageMeta.LengthByteIndex];
            hasCommand = true;
            break;
        }

        if (!hasCommand)
        {
            commandResponse = CommandResponse.CommandIncorrect;
            return null;
        }
        
        if (length > PackageMeta.PayloadMaxByte)
        {
            commandResponse = CommandResponse.PackageIncorrect;
            return null;
        }

        var command = (Command)data[PackageMeta.CommandByteIndex];
        
        var payload = data.Slice(PackageMeta.PackagePayloadIndex, length).ToArray();

        commandResponse = CommandResponse.OK;
        return (command, payload);
    }
}