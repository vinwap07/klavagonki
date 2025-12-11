using System.Net;
using System.Net.Sockets;

public class KlavagonkiClient : IDisposable
{
    public readonly Socket Socket;
    private readonly string _serverIp;
    private readonly int _serverPort;
    private bool _isConnected;
    private bool _isDisposed;

    public event Action<byte[]>? OnDataReceived;
    public event Action<Exception>? OnConnectionError;
    public event Action? OnDisconnected;

    public bool IsConnected => _isConnected && Socket.Connected;
    public string ServerIp => _serverIp;
    public int ServerPort => _serverPort;

    public KlavagonkiClient(string serverIp = "127.0.0.1", int serverPort = 8888)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _isConnected = false;
        _isDisposed = false;
    }
    
    public async Task ConnectAsync()
    {
        ThrowIfDisposed();

        if (_isConnected) return;

        try
        {
            IPAddress ipAddress = IPAddress.Parse(_serverIp);
            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, _serverPort);

            await Socket.ConnectAsync(remoteEndPoint);
            _isConnected = true;

            _ = Task.Run(ListenForResponses);
        }
        catch (Exception ex)
        {
            _isConnected = false;
            OnConnectionError?.Invoke(ex);
            throw;
        }
    }

    private async void ListenForResponses()
    {
        byte[] buffer = new byte[1024];

        while (_isConnected && Socket.Connected && !_isDisposed)
        {
            try
            {
                int bytesReceived = await Socket.ReceiveAsync(buffer, SocketFlags.None);

                if (bytesReceived == 0)
                {
                    Disconnect();
                    break;
                }

                if (bytesReceived > 0)
                {
                    byte[] receivedData = new byte[bytesReceived];
                    Array.Copy(buffer, receivedData, bytesReceived);

                    OnDataReceived?.Invoke(receivedData);
                }
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (_isConnected && !_isDisposed)
                {
                    OnConnectionError?.Invoke(ex);
                    Disconnect();
                }

                break;
            }
        }
    }
    public async Task SendAsync(byte[] data)
    {
        ThrowIfDisposed();

        if (!IsConnected) throw new InvalidOperationException("Client is not connected to server");

        if (data == null || data.Length == 0) throw new ArgumentException("Data cannot be null or empty");

        try
        {
            await Socket.SendAsync(data, SocketFlags.None);
        }
        catch (Exception ex)
        {
            OnConnectionError?.Invoke(ex);
            throw;
        }
    }
    public void Disconnect()
    {
        if (!_isConnected) return;

        try
        {
            Socket.Shutdown(SocketShutdown.Both);
        }
        catch
        {
        }
        finally
        {
            try
            {
                Socket.Close();
            }
            catch
            {
            }

            _isConnected = false;
            OnDisconnected?.Invoke();
        }
    }

    public async Task SendCommandAsync(Command command, Dictionary<string, string>? data = null)
    {
        var payload = PayloadSerializer.Encode(data);
        var package = new PackageBuilder(payload, command).Build();
        await SendAsync(package);
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(KlavagonkiClient));
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        Disconnect();

        try
        {
            Socket.Dispose();
        }
        catch
        {
        }

        GC.SuppressFinalize(this);
    }

    ~KlavagonkiClient()
    {
        Dispose();
    }
}