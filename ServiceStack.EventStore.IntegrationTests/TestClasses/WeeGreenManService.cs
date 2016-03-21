using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack.EventStore.IntegrationTests.TestClasses
{
    public class WeeGreenManService: Service
    {
        public void Any(WeeGreenMenLanded @event)
        {
            Console.WriteLine("here");
        }
    }
}
