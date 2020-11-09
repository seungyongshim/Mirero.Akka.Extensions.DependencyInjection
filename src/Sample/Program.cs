using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.DI.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    public class ParentActor : ReceiveActor
    {
        public ParentActor()
        {
            var childActor1 = Context.ActorOf(Context.DI().PropsFactory<ChildActor>().Create(), "Child1");
            var childActor2 = Context.ActorOf(Context.DI().PropsFactory<ChildActor>().Create(), "Child2");

            childActor1.Tell("Hello, Kid");
            childActor2.Tell("Hello, Kid");
        }
    }

    public class ChildActor : ReceiveActor
    {
        public ChildActor()
        {
            Receive<string>(_ => { });
        }
    }
}
