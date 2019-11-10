using Akka.Actor;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;


namespace AkkaExample
{
    class Program
    {
        public static IServiceProvider ServiceProvider;

        static void Main(string[] args)
        {
            int minThread;
            int completionPortThread;
            //ThreadPool.SetMinThreads(100, 100);
            ThreadPool.GetMinThreads(out minThread, out completionPortThread);
            Console.WriteLine($"min Thread:{minThread} - completion:{completionPortThread}");
            var services = ConfigureServices();

            //var serviceProvider = new ServiceCollection()
            //    .Add(services)
            //    .BuildServiceProvider();


            var builder = new ContainerBuilder();
            builder.Populate(services);
            //builder.RegisterType<Test>().As<IHttpClientFactory>();
            var container = builder.Build();

            // Generate a provider

            var actorSystems = ActorSystem.Create("test-actor-system");
            actorSystems.UseAutofac(container);

            for (int i = 1; i < 2; i++)
            {
                var inventoryActor = actorSystems.ActorOf(Props.Create<OrderActor>(args: container), $"order-actor-{i}");
                inventoryActor.Tell(new Order()
                {
                    Id = i
                });
            }

            Console.ReadLine();
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient("mock",
                c => { c.BaseAddress = new Uri("http://www.mocky.io/v2/5dc5096f3200007d0076962d"); });

            return services;
        }
    }

    public interface ITest
    {
        void Write();
    }


    public class Test : ITest
    {
        public void Write()
        {
            Console.WriteLine("x");
        }
    }
}
