// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel.Discovery;
using System.Threading;

namespace ServiceModelEx
{
   public class DiscoveredServices<T> : AddressesContainer<T> where T : class
   {
      Thread m_WorkerThread;
 
      bool Terminate 
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get;
         [MethodImpl(MethodImplOptions.Synchronized)]
         set;
      }

      public DiscoveredServices(DiscoveredServices<T> container = null) : base(container)
      {}
      public override void Open()
      {
         m_WorkerThread = new Thread(Discover);
         m_WorkerThread.Start();
      }
      public override void Close()
      {
         Terminate = true;
         m_WorkerThread.Join();
      }
      public void Abort()
      {
         Terminate = true;
         Thread.Sleep(0);
         m_WorkerThread.Abort();
         m_WorkerThread.Join();
      }
      void Discover()
      {
         while(Terminate == false)
         {
            FindResponse discovered = Find();

            lock(this)
            {
               Dictionary.Clear();
               foreach(EndpointDiscoveryMetadata endpoint in discovered.Endpoints)
               {
                  Dictionary[endpoint.Address] = endpoint.Scopes.ToArray();
               }
            }
         }
      }
      protected virtual FindResponse Find()
      {
         DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
         FindCriteria criteria = new FindCriteria(typeof(T));
         FindResponse response = discoveryClient.Find(criteria);
         discoveryClient.Close();
         return response;
      }
   }
}


