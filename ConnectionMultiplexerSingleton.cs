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

        public static IDatabase Database
        {
            get
            {
                if (_database == null)
                {
                    lock (lockObject)
                    {
                        if (_database == null)
                        {
                            var conn = ConnectionMultiplexer.Connect(option);
                            _database = conn.GetDatabase(0);
                            Console.WriteLine("kaç");
                        }
                    }
                }

                return _database;
            }
        }

        //private static IDatabase _instance;
        private static IDatabase _database;

    }
}
