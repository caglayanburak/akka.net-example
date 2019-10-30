using System;
using System.Collections.Generic;
using Akka.Actor;

namespace AkkaExample
{
    public class OrderActor : UntypedActor
    {
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 5,
                withinTimeRange: TimeSpan.FromMinutes(1),
                localOnlyDecider: ex =>
                {
                    switch (ex)
                    {
                        case ArithmeticException ae:
                            return Directive.Resume;
                        case NullReferenceException ne:
                            return Directive.Restart;
                        default:
                            return Directive.Stop;
                    }
                });
        }
        protected override void OnReceive(object message)
        {

            var order = (Order)message;
            Console.WriteLine($"order came {order.Id}");


            var orderDetails = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            foreach (var orderDetailId in orderDetails)
            {
                var orderDetailActor = Context.ActorOf(Props.Create<OrderDetailActor>(), $"order-detail-actor-{orderDetailId}");

                var orderDetActor = orderDetailActor.Ask(new OrderDetail
                {
                    Id = orderDetailId,
                    OrderId = order.Id
                }).Result;

                Console.WriteLine($"task result:" + ((OrderDetail)orderDetActor).Message);
            }


        }
    }
}