using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain.Models;

namespace Domain;

public interface IServerCommandHandler
{
    Task Invoke(Socket sender,  Dictionary<string, string> parameters, ConcurrentDictionary<string, Room> rooms, CancellationToken ct = default);
}