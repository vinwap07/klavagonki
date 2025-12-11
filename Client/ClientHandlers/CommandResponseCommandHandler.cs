using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Client.ClientHandlers;

[Command(Command.CommandResponse)]
public class CommandResponseCommandHandler : IClientCommandHandler
{
    public Task Invoke(Socket sender, Dictionary<string, string> parameters, CancellationToken ct = default)
    {
        var status = parameters["status"];
        if (status == ((byte)CommandResponse.OK).ToString())
        {
            ConsoleUi.Success("everything is ok");
        }
        else
        {
            ConsoleUi.Error(status);
        }
        return Task.CompletedTask;
    }
}