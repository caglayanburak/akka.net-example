using Akka.Actor;
using Autofac;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AkkaExample
{
    public class OrderActor : ReceiveActor
    {
        private readonly IContainer _container;
        private IDatabase _database;
        private string channelPattern = "order-actor*";

        public OrderActor(IContainer container)
        {
            _container = container;
            _database = ConnectionMultiplexerSingleton.Connection.GetDatabase(0);

            Receive<Order>(order =>
            {
                Console.WriteLine($"order came {order.Id}");

                _database.StringSet($"orderdetail-{order.Id}", 0);
                var orderDetails = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

                for (int i = 11; i <= order.OrderDetailCount; i++)
                {
                    orderDetails.Add(i);
                }

                foreach (var orderDetailId in orderDetails)
                {
                    var orderDetailActor = Context.ActorOf(Props.Create<OrderDetailActor>(args: _container), $"order-detail-actor-{orderDetailId}");

                    //Ask 
                    //var result = orderDetailActor.Ask<OrderDetail>(new OrderDetail
                    //{
                    //    Id = orderDetailId,
                    //    OrderId = order.Id
                    //});

                    //Console.WriteLine($"Order-Actor => task result:{orderDetailId} \n Time: {DateTime.Now.ToString("hh:mm:ss.fff")} \n Thread Id: {Thread.CurrentThread.ManagedThreadId} \n Result:{result.Result.Message}");

                    //Tell
                    var message = new OrderDetail
                    {
                        Id = orderDetailId,
                        OrderId = order.Id,
                        Order = order
                    };
                    orderDetailActor.Tell(new Retry(message, 5));

                    //Sender.Tell(orderDetailActor);

                    Console.WriteLine($"Order-Actor => task result:{orderDetailId} \n Time: {DateTime.Now.ToString("hh:mm:ss.fff")} \n Thread Id: {Thread.CurrentThread.ManagedThreadId}");
                }
            });
        }


        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 5,
                withinTimeRange: TimeSpan.FromSeconds(10),
                localOnlyDecider: ex =>
                {
                    switch (ex)
                    {
                        case ApplicationException ae:

                            Console.WriteLine("*****Go go go");
                            return Directive.Resume;
                        case NullReferenceException ne:
                            Console.WriteLine("*****************Null Go go go");
                            return Directive.Restart;
                        default:
                            return Directive.Stop;
                    }

                });
        }


    }
}