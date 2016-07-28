// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ServiceModelEx
{
   public class AsyncContextScope : IDisposable
   {
      public AsyncContext AsyncContext
      {
         [MethodImpl(MethodImplOptions.Synchronized)]
         get;
         [MethodImpl(MethodImplOptions.Synchronized)]
         private set;
      }

      public AsyncContextScope()
      {
         if(AsyncContext == null)
         {
            AsyncContext = new AsyncContext();
         }
      }
      public AsyncContextScope(AsyncContextScope asyncContextScope)
      {
         Debug.Assert(asyncContextScope != null);

         AsyncContext = new AsyncContext(asyncContextScope.AsyncContext);
         AsyncContext.Restore();
      }

      public void Dispose()
      {
         Close();
      }
      public void Close()
      {
         AsyncContext.Restore();
      }
   }
}
