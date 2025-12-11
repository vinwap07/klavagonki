using System.Collections.Concurrent;
using System.Net.Sockets;
using Domain;
using Domain.Models;

namespace Client.ClientHandlers;

[Command(Command.CheckChar)]
public class CheckCharCommandHandler : IClientCommandHandler
{
    public Task Invoke(Socket sender, Dictionary<string, string> parameters, CancellationToken ct = default)
    {
        var isCorrect = bool.Parse(parameters["isCorrect"]);
        if (isCorrect)
        {
            ConsoleUi.Success("+");
        }
        else
        {
            ConsoleUi.Error("-");
        }
        return Task.CompletedTask;
    }
}