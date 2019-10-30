using System;
using Akka.Actor;

namespace AkkaExample
{
    public class OrderDetailActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            var orderDetail = (OrderDetail)message;

            //Api call for data and validation

            // if (orderDetail.Id == 8 || orderDetail.Id == 5)
            // {
            //     throw new ArithmeticException("test");
            // }

            Console.WriteLine($"Order detail message came Id:{orderDetail.Id}");
            orderDetail.Message = "test";
            Sender.Tell(orderDetail, Self);

        }
    }
}