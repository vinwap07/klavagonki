using Client;

var client = new KlavagonkiClient("127.0.0.1", 8080);
await client.Start();