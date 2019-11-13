using Akka.Persistence;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using Akka.Persistence.MongoDb;

namespace AkkaExample
{
    public class ExampleActor : ReceivePersistentActor
    {
        public override string PersistenceId => "ExampleActor";

        private ILoggingAdapter logger = Context.GetLogger();

        public ExampleActor()
        {
            this.Command<string>(msg => this.Persist(msg, _ => this.logger.Info(msg)));
            this.Recover<string>(msg => this.logger.Info($"Recovered: {msg}"));
        }
    }

    public class ExampleActorSystem
    {
        public static ActorSystem ExampleSystem { get; private set; }

        public static IActorRef ExampleActor { get; private set; }

        public static void Start()
        {
            var config = ConfigurationFactory.ParseString(@"
                akka.persistence {
                    journal {
                        plugin = ""akka.persistence.journal.mongodb""
                        mongodb {
                            connection-string = ""mongodb://127.0.0.1:27017/example""
                        }
                    }
                }");

            ExampleSystem = ActorSystem.Create("ExampleSystem", config);

            MongoDbPersistence.Get(ExampleSystem);

            ExampleActor = ExampleSystem.ActorOf<ExampleActor>();
        }

        public static void Stop()
        {
            ExampleSystem.Terminate().Wait();
        }
    }
}
