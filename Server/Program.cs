using System.Net;

IPAddress ip = IPAddress.Parse("127.0.0.1");
IPEndPoint endpoint = new IPEndPoint(ip, 8080);
var server = new KlavagonkiServer(endpoint);
await server.StartServer();