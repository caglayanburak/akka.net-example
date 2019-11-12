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
        private string channelPattern = "order-actor*";

        public OrderDetailActor(IContainer container)
        {
            _container = container;
            _factory = _container.Resolve<IHttpClientFactory>();
            _client = _factory.CreateClient("mock");
            _database = ConnectionMultiplexerSingleton.Connection.GetDatabase(0);
            Receive<Retry>(retry =>
            {
                //Api call for data and validation
                var orderDetail = retry.Message as OrderDetail;
                //if (ConnectionMultiplexerSingleton.retryCount < 1 && orderDetail.OrderId == 1 && (orderDetail.Id == 8))
                //{
                //    throw new NullReferenceException("test");
                //}


                HttpResponseMessage response = _client.GetAsync(string.Empty).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(
                    $"Order detail message came Id:{orderDetail.Id} - {responseBody} \n Time:{DateTime.Now.ToString("hh:mm:ss.fff")} \n ThreadId: {Thread.CurrentThread.ManagedThreadId}");
                orderDetail.Message = $"test-{orderDetail.Id}";



                var r = _database.StringIncrementAsync($"orderdetail-{orderDetail.OrderId}").Result;
                //Sender.Tell(orderDetail, Self);//for Ask method
                var publisher = ConnectionMultiplexerSingleton.Connection.GetSubscriber();
                publisher.Publish($"order-actor-{ orderDetail.OrderId}", r);
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
            Retry oldRetry;
            var retry = (oldRetry = message as Retry) != null
                ? new Retry(oldRetry.Message, oldRetry.Ttl - 1)
                : new Retry(message, 5);

            if (retry.Ttl > 0)
                Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(10), Self, retry, Sender);

            base.PreRestart(reason, message);
            //ConnectionMultiplexerSingleton.retryCount++;
            //Self.Tell(message);
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