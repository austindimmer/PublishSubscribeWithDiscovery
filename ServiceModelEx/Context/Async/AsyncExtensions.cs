// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Threading.Tasks;

namespace ServiceModelEx
{
   public static class AsyncExtensions
   {
      public static WcfContextAwaiter FlowWcfContext(this Task task)
      {
         return new WcfContextAwaiter(task);
      }
      public static WcfContextAwaiter<T> FlowWcfContext<T>(this Task<T> task)
      {
         return new WcfContextAwaiter<T>(task);
      }
   }
}
