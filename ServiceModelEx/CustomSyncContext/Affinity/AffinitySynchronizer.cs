// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Security.Permissions;
using System.Threading;

namespace ServiceModelEx
{
   [SecurityPermission(SecurityAction.Demand,ControlThread = true)]
   public class AffinitySynchronizer : ThreadPoolSynchronizer
   {
      public AffinitySynchronizer() : this("AffinitySynchronizer Worker Thread")
      {}

      public AffinitySynchronizer(string threadName) : base(1,threadName)
      {}
      public override SynchronizationContext CreateCopy()
      {
         return this;
      }
   }
}