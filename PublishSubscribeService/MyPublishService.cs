﻿using ServiceLibrary.Contracts;
using ServiceModelEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PublishSubscribeService
{
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, IncludeExceptionDetailInFaults = DebugHelper.IncludeExceptionDetailInFaults, InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = false)]
    class MyPublishService : DiscoveryPublishService<IMyEvents>, IMyEvents
    {
        public void OnEvent1()
        {
            Debug.WriteLine("MyPublishService OnEvent1");
            FireEvent();
        }
        public void OnEvent2(int number)
        {
            Debug.WriteLine("MyPublishService OnEvent2");
            FireEvent(number);
        }
        public void OnEvent3(int number, string text)
        {
            Debug.WriteLine("MyPublishService OnEvent3");
            FireEvent(number, text);
        }
    }
}
