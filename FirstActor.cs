using System;
using Akka.Actor;

namespace AkkaExample
{
    public class FirstActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            Console.WriteLine($"Message received {message}");
        }

        protected override void PreStart() => Console.WriteLine("Started actor");

        protected override void PostStop() => Console.WriteLine("Stopped actor");
    }
}