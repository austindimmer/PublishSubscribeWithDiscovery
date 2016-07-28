using System;
using System.Linq;
using System.ServiceModel;

namespace ServiceLibrary.Contracts
{
    [ServiceContract]
    public interface IMyEvents
    {
        [OperationContract(IsOneWay = true)]
        void OnEvent1();
        [OperationContract(IsOneWay = true)]
        void OnEvent2(int number);
        [OperationContract(IsOneWay = true)]
        void OnEvent3(int number, string text);
    }
}
