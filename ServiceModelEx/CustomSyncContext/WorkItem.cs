// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Threading;

namespace ServiceModelEx
{
   [Serializable]
   internal class WorkItem
   {
      ManualResetEvent m_AsyncWaitHandle;

      public object State
      {get;private set;}

      public SendOrPostCallback Method
      {get;private set;}

      public AsyncContext AsyncContext
      {get;private set;}

      public WaitHandle AsyncWaitHandle
      {
         get
         {
            return m_AsyncWaitHandle;
         }
      }

      internal WorkItem(SendOrPostCallback method,object state,AsyncContext asyncContext = null)
      {
         Method = method;
         State = state;
         m_AsyncWaitHandle = new ManualResetEvent(false);
         AsyncContext = asyncContext;
      }

      //This method is called on the worker thread to execute the method
      internal void CallBack()
      {
         Method(State);
         m_AsyncWaitHandle.Set();
      }
   }
}