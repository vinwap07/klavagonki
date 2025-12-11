using System.Net.Sockets;

namespace Domain;

public interface IClientCommandHandler
{
    Task Invoke(Socket sender,  Dictionary<string, string> parameters, CancellationToken ct = default);
}