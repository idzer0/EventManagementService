
public class RedisSettings
{
    public string Server = "localhost";
    public int Port = 6379;
    public string Password = "secret";
    public int ConnectTimeout = 5000;
    public int SyncTimeout = 3000;
    public bool AbortOnConnectFail = false;
    public int ReconnectRetryPolicy = 5000;
}