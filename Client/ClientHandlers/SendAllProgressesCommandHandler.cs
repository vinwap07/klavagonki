using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Client.ClientHandlers;

[Command(Command.SendAllProgresses)]
public class SendAllProgressesCommandHandler : IClientCommandHandler
{
    public Task Invoke(Socket sender, Dictionary<string, string> parameters, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}