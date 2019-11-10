using Akka.Actor;
using Autofac;
using StackExchange.Redis;
using System;
using System.Net.Http;
using System.Threading;

namespace AkkaExample
{
    public class OrderDetailActor : UntypedActor
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
            Self.Forward(message);
            ConnectionMultiplexerSingleton.retryCount++;
        }

        protected override void PostStop()
        {
            base.PostStop();
        }

        protected override void OnReceive(object message)
        {

            var orderDetail = (OrderDetail)message;

            //Api call for data and validation

            if (ConnectionMultiplexerSingleton.retryCount < 6 && orderDetail.OrderId == 1 && (orderDetail.Id == 8))
            {
                throw new NullReferenceException("test");
            }

            HttpResponseMessage response = _client.GetAsync(string.Empty).Result;
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Order detail message came Id:{orderDetail.Id} - {responseBody} \n Time:{DateTime.Now.ToString("hh:mm:ss.fff")} \n ThreadId: {Thread.CurrentThread.ManagedThreadId}");
            orderDetail.Message = $"test-{orderDetail.Id}";



            _database.StringIncrementAsync($"orderdetail-{orderDetail.OrderId}");
            //Sender.Tell(orderDetail, Self);//for Ask method

        }
    }
}