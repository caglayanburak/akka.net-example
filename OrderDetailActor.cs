using Akka.Actor;
using Autofac;
using StackExchange.Redis;
using System;
using System.Net.Http;
using System.Threading;

namespace AkkaExample
{
    public class OrderDetailActor : ReceiveActor
    {
        private IHttpClientFactory _factory;
        private readonly IContainer _container;
        private HttpClient _client;
        private IDatabase _database;

        public OrderDetailActor(IContainer container)
        {
            _container = container;
            _factory = _container.Resolve<IHttpClientFactory>();
            _client = _factory.CreateClient("mock");
            _database = ConnectionMultiplexerSingleton.Database;

            Receive<OrderDetail>(orderDetail =>
            {
                //Api call for data and validation

                if (ConnectionMultiplexerSingleton.retryCount < 1 && orderDetail.OrderId == 1 && (orderDetail.Id == 8))
                {
                    throw new NullReferenceException("test");
                }

                if (orderDetail.Id == 8)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("geldi");
                }

                HttpResponseMessage response = _client.GetAsync(string.Empty).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(
                    $"Order detail message came Id:{orderDetail.Id} - {responseBody} \n Time:{DateTime.Now.ToString("hh:mm:ss.fff")} \n ThreadId: {Thread.CurrentThread.ManagedThreadId}");
                orderDetail.Message = $"test-{orderDetail.Id}";



                _database.StringIncrementAsync($"orderdetail-{orderDetail.OrderId}");
                //Sender.Tell(orderDetail, Self);//for Ask method

            });

        }

        protected override void PostRestart(Exception reason)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("*******PostRestart");
        }

        protected override void PreRestart(Exception reason, object message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("*******PreRestart");
            
            base.PreRestart(reason, message);
            //ConnectionMultiplexerSingleton.retryCount++;
            Self.Tell(message);
            //Self.Forward(message);
        }

        protected override void PostStop()
        {
            Console.ForegroundColor = ConsoleColor.Blue;

            Console.WriteLine("*******PostStop");
            base.PostStop();
        }
    }

    sealed class Retry
    {
        public readonly object Message;
        public readonly int Ttl;

        public Retry(object message, int ttl)
        {
            Message = message;
            Ttl = ttl;
        }
    }
}