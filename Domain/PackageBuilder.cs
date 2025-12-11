public class PackageBuilder
{
    private byte[] _package;

    public PackageBuilder(byte[] content, Command command)
    {
        if (content.Length > PackageMeta.PayloadMaxByte)
        {
            throw new ArgumentException("Package size is too large.");
        }
        
        _package = new byte[4 + content.Length];
        CreateBasePackage(content, command);
    }

    private void CreateBasePackage(byte[] content, Command command)
    {
        _package[0] = PackageMeta.Start;
        _package[^1] = PackageMeta.End;
        _package[PackageMeta.CommandByteIndex] = (byte) command;
        _package[PackageMeta.LengthByteIndex] = (byte) content.Length;
        Array.Copy(content, 0, _package, PackageMeta.PackagePayloadIndex, content.Length);
    }
    
    public byte[] Build() => _package;
}