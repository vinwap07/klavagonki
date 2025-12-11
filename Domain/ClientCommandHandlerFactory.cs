using System.Reflection;
using System.Windows.Input;

namespace Domain;

public class ClientCommandHandlerFactory
{
    private static readonly Lazy<Dictionary<Command, IClientCommandHandler>> CommandHandlers =
        new(BuildAllHandlers);

    private static Dictionary<Command, IClientCommandHandler> BuildAllHandlers()
    {
        var allHandlers = new Dictionary<Command, IClientCommandHandler>();
        var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type is { IsClass : true, IsAbstract: false } 
                           && typeof(IClientCommandHandler).IsAssignableFrom(type));
        foreach (var handlerType in handlerTypes)
        {
            var command = handlerType.GetCustomAttribute<CommandAttribute>()!.Command ;
            var handler = (IClientCommandHandler)Activator.CreateInstance(handlerType)!;
            allHandlers.Add(command, handler);
        }
        return allHandlers;
    }

    public static IClientCommandHandler GetHandler(Command command)
    {
        return CommandHandlers.Value.TryGetValue(command, out var handler) 
            ? handler
            : throw new NotSupportedException($"The command '{command}' is not supported.");
    }
}