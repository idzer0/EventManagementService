
public class RedisSettings
{
    public string Server {get; set;} = "localhost";
    public int Port {get; set;} = 6379;
    public string Password {get; set;} = "secret";
    public int ConnectTimeout {get; set;} = 5000;
    public int SyncTimeout {get; set;} = 3000;
    public bool AbortOnConnectFail {get; set;} = false;
    public int ReconnectRetryPolicy {get; set;} = 5000;
    public int DefaultTimeSpanMinutes {get; set;} = 60;
}