using System;
using Akka.Actor;

namespace AkkaExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var actorSystems = ActorSystem.Create("test-actor-system");
            var inventoryActor = actorSystems.ActorOf(Props.Create<OrderActor>(), "order-actor");
            inventoryActor.Tell(new Order()
            {
                Id = 1
            });

            Console.ReadLine();
        }
    }
}
