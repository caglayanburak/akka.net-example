using Akka.Actor;
using Autofac;
using System;
using System.Collections.Generic;
using System.Threading;
using StackExchange.Redis;

namespace AkkaExample
{
    public class OrderActor : ReceiveActor
    {
        private readonly IContainer _container;
        private IDatabase _database;

        public OrderActor(IContainer container)
        {
            _container = container;
            _database = ConnectionMultiplexerSingleton.Database;

            Receive<Order>(order =>
            {
                Console.WriteLine($"order came {order.Id}");

                _database.StringSet($"orderdetail-{order.Id}", 0);
                var orderDetails = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

                for (int i = 11; i <= 11; i++)
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
                    orderDetailActor.Tell(new OrderDetail
                    {
                        Id = orderDetailId,
                        OrderId = order.Id
                    });

                    //Sender.Tell(orderDetailActor);

                    Console.WriteLine($"Order-Actor => task result:{orderDetailId} \n Time: {DateTime.Now.ToString("hh:mm:ss.fff")} \n Thread Id: {Thread.CurrentThread.ManagedThreadId}");
                }
            });
        }
        

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries:4,
                withinTimeRange:TimeSpan.FromMinutes(5), 
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