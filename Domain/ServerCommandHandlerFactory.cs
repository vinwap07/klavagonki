using System.Reflection;
using System.Windows.Input;

namespace Domain;

public class ServerCommandHandlerFactory
{
    private static readonly Lazy<Dictionary<Command, IServerCommandHandler>> CommandHandlers =
        new(BuildAllHandlers);

    private static Dictionary<Command, IServerCommandHandler> BuildAllHandlers()
    {
        var allHandlers = new Dictionary<Command, IServerCommandHandler>();
        var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type is { IsClass : true, IsAbstract: false } 
                && typeof(IServerCommandHandler).IsAssignableFrom(type));
        foreach (var handlerType in handlerTypes)
        {
            var command = handlerType.GetCustomAttribute<CommandAttribute>()!.Command ;
            var handler = (IServerCommandHandler)Activator.CreateInstance(handlerType)!;
            allHandlers.Add(command, handler);
        }
        return allHandlers;
    }

    public static IServerCommandHandler GetHandler(Command command)
    {
        return CommandHandlers.Value.TryGetValue(command, out var handler) 
            ? handler
            : throw new NotSupportedException($"The command '{command}' is not supported.");
    }
}