using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Domain.Models;
using Domain;

public class KlavogonkiServer
{
   private readonly Socket _server;
   private ConcurrentDictionary<string, Room> _rooms = new();

   public KlavogonkiServer(IPEndPoint endPoint)
   {
      _server = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      _server.Bind(endPoint);
      _server.Listen(10);
   }

   public async Task StartServer()
   {
      var cts = new CancellationTokenSource();
      try
      {
         while (true)
         {
            var clientSocket = await _server.AcceptAsync();
            Console.WriteLine($"Новое подключение: {clientSocket.RemoteEndPoint}");
      
            _ = Task.Run(() => HandleConnection(clientSocket, cts), cts.Token);
         }
      }
      catch
      {
         await _server.DisconnectAsync(true, cts.Token);
      }
   }

   private async Task HandleConnection(Socket connection, CancellationTokenSource cts)
   {
      await connection.SendAsync(
         new PackageBuilder(
               PayloadSerializer.Encode(
                  new Dictionary<string, string>()
                     {
                        { "status", ((byte) CommandResponse.OK).ToString() }
                     }), 
               Command.CommandResponse)
            .Build(), 
         SocketFlags.None);
      
      
      ArraySegment<byte> buffer = new byte[128];

      do
      {
         var messageBytesCount = await connection.ReceiveAsync(buffer, SocketFlags.None);
         var message = buffer[..messageBytesCount];
         var response = PackageParser.TryParse(message, out var commandResponse);

         if (response == null || commandResponse != CommandResponse.OK)
         {
            await connection.SendAsync(
               new PackageBuilder(
                     PayloadSerializer.Encode(
                        new Dictionary<string, string>()
                        {
                           { "status", ((byte) commandResponse).ToString() }
                        }),
                     Command.CommandResponse)
                  .Build(),
               SocketFlags.None);
            continue;
         }

         await HandleClient(connection, response.Value.Command, response.Value.Payload, cts);
      }
      while (!cts.IsCancellationRequested || connection.Connected);
      
      
   }

   private async Task HandleClient(Socket connection, Command command, byte[] payload, CancellationTokenSource cts)
   {
      var commandHandler = ServerCommandHandlerFactory.GetHandler(command);
      var parameters = PayloadSerializer.Decode(payload);
      await commandHandler.Invoke(connection, parameters, _rooms, cts.Token);
   }
}