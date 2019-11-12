using StackExchange.Redis;
using System;

namespace AkkaExample
{
    public static class ConnectionMultiplexerSingleton
    {
        public static int retryCount = 0;
        static ConfigurationOptions option = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            ConnectRetry = 3,
            DefaultDatabase = 0,
            EndPoints = { "127.0.0.1" }
        };

        private static object lockObject = new object();

        public static ConnectionMultiplexer Connection
        {
            get
            {
                if (_connection == null)
                {
                    lock (lockObject)
                    {
                        if (_connection == null)
                        {
                          _connection = ConnectionMultiplexer.Connect(option);
                        }
                    }
                }

                return _connection;
            }
        }

        //private static IDatabase _instance;
        private static ConnectionMultiplexer _connection;

    }
}
