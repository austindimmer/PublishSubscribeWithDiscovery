using ServiceLibrary.Contracts;
using ServiceModelEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublishSubscribeService
{
    class MyPublishService : DiscoveryPublishService<IMyEvents>, IMyEvents
    {
        public void OnEvent1()
        {
            FireEvent();
        }
        public void OnEvent2(int number)
        {
            FireEvent(number);
        }
        public void OnEvent3(int number, string text)
        {
            FireEvent(number, text);
        }
    }
}
