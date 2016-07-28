using ServiceModelEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Contracts
{
    
    [ServiceContract(CallbackContract = typeof(IMyEvents))]
    public interface IMySubscriptionService : ISubscriptionService
    { }
}
