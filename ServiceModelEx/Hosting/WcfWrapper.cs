// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.ServiceModel;

namespace ServiceModelEx
{
   public abstract class WcfWrapper<S,I> : IDisposable,ICommunicationObject
                                                       where I : class 
                                                       where S : class,I
   {
      protected I Proxy
      {get;private set;}

      protected WcfWrapper()
      {
         Proxy = InProcFactory.CreateInstance<S,I>();
      }
   
      protected WcfWrapper(S singleton)
      {
         InProcFactory.SetSingleton(singleton);
         Proxy = InProcFactory.CreateInstance<S,I>();
      }
      public void Dispose()
      {
         Close();
      }

      public void Close()
      {
         InProcFactory.CloseProxy(Proxy);
      }

      void ICommunicationObject.Abort()
      {
         (Proxy as ICommunicationObject).Abort();
      }

      IAsyncResult ICommunicationObject.BeginClose(TimeSpan timeout,AsyncCallback callback,object state)
      {
         return (Proxy as ICommunicationObject).BeginClose(timeout,callback,state);
      }

      IAsyncResult ICommunicationObject.BeginClose(AsyncCallback callback,object state)
      {
         return (Proxy as ICommunicationObject).BeginClose(callback,state);
      }

      IAsyncResult ICommunicationObject.BeginOpen(TimeSpan timeout,AsyncCallback callback,object state)
      {
         return (Proxy as ICommunicationObject).BeginOpen(timeout,callback,state);
      }

      IAsyncResult ICommunicationObject.BeginOpen(AsyncCallback callback,object state)
      {
         return (Proxy as ICommunicationObject).BeginOpen(callback,state);
      }

      void ICommunicationObject.Close(TimeSpan timeout)
      {
         (Proxy as ICommunicationObject).Close(timeout);
      }

      void ICommunicationObject.Close()
      {
         (Proxy as ICommunicationObject).Close();
      }

      event EventHandler ICommunicationObject.Closed
      {
         add
         {
            (Proxy as ICommunicationObject).Closed += value;
         }
         remove
         {
            (Proxy as ICommunicationObject).Closed -= value;
         }
      }

      event EventHandler ICommunicationObject.Closing
      {
         add
         {
            (Proxy as ICommunicationObject).Closing += value;
         }
         remove
         {
            (Proxy as ICommunicationObject).Closing -= value;
         }
      }

      void ICommunicationObject.EndClose(IAsyncResult result)
      {
         (Proxy as ICommunicationObject).EndClose(result);
      }

      void ICommunicationObject.EndOpen(IAsyncResult result)
      {
         (Proxy as ICommunicationObject).EndOpen(result);
      }

      event EventHandler ICommunicationObject.Faulted
      {
         add
         {
            (Proxy as ICommunicationObject).Faulted += value;
         }
         remove
         {
            (Proxy as ICommunicationObject).Faulted -= value;
         }
      }

      void ICommunicationObject.Open(TimeSpan timeout)
      {
         (Proxy as ICommunicationObject).Open(timeout);
      }

      void ICommunicationObject.Open()
      {
         (Proxy as ICommunicationObject).Open();
      }

      event EventHandler ICommunicationObject.Opened
      {
         add
         {
            (Proxy as ICommunicationObject).Opened += value;
         }
         remove
         {
            (Proxy as ICommunicationObject).Opened -= value;
         }
      }

      event EventHandler ICommunicationObject.Opening
      {
         add
         {
            (Proxy as ICommunicationObject).Opening += value;
         }
         remove
         {
            (Proxy as ICommunicationObject).Opening -= value;
         }
      }

      CommunicationState ICommunicationObject.State
      {
         get
         {
            return (Proxy as ICommunicationObject).State;
         }
      }
   }
}