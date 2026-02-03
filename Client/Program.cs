using Client;

var game = new GameManager("127.0.0.1", 8080);
await game.StartGame();