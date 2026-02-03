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
        
        var assemblies = new[] 
        { 
            Assembly.GetExecutingAssembly(),
            Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()
        };
        
        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(type => type is { IsClass: true, IsAbstract: false } 
                               && typeof(IServerCommandHandler).IsAssignableFrom(type));
            
            foreach (var handlerType in handlerTypes)
            {
                var attribute = handlerType.GetCustomAttribute<CommandAttribute>();
                if (attribute == null)
                {
                    continue;
                }
                try
                {
                    var handler = (IServerCommandHandler)Activator.CreateInstance(handlerType)!;
                    allHandlers[attribute.Command] = handler;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка создания обработчика {handlerType.Name}: {ex.Message}");
                }
            }
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