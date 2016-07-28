// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;

#pragma warning disable 618

namespace ServiceModelEx.ServiceFabric.Services
{
   public class ServiceOperationInvoker : IOperationInvoker
   {
      readonly IOperationInvoker m_OldInvoker;
      readonly MethodInfo m_MethodInfo;

      public ServiceOperationInvoker(IOperationInvoker oldInvoker,OperationDescription operationDescription)
      {
         Debug.Assert(oldInvoker != null);

         m_OldInvoker = oldInvoker;
         m_MethodInfo = operationDescription.TaskMethod;
      }
      public virtual object[] AllocateInputs()
      {
         return m_OldInvoker.AllocateInputs();
      }
      public bool IsSynchronous
      {
         get
         {
            return m_OldInvoker.IsSynchronous;
         }
      }

      public object Invoke(object instance,object[] inputs,out object[] outputs)
      {
         return m_OldInvoker.Invoke(instance,inputs,out outputs);
      }
      public IAsyncResult InvokeBegin(object instance,object[] inputs,AsyncCallback callback,object state)
      {
         IAsyncResult result = null;
         try
         {
            result = m_OldInvoker.InvokeBegin(instance,inputs,callback,state);
         }
         catch(Exception exception)
         {
            Debug.Assert(true,"Invoker exception: " + exception.Message);
            throw exception;
         }
         return result;
      }

      Exception PreserveOriginalException(Exception source)
      {
         Exception result = null;
         MemoryStream m_Stream = new MemoryStream();
         BinaryFormatter formatter = new BinaryFormatter();

         m_Stream.Position = 0;
         using (m_Stream)
         {
               formatter.Serialize(m_Stream, source);
               m_Stream.Position = 0;
               result = formatter.Deserialize(m_Stream) as Exception;
         }
         return result;
      }
      public object InvokeEnd(object instance,out object[] outputs,IAsyncResult result)
      {
         object returnedValue = null;
         object[] outputParams = {};
         Exception exception = null;

         try
         {
            Task task = (result as Task);
            if(task.Status == TaskStatus.Faulted)
            {
               //Preserve original stack trace.
               exception = PreserveOriginalException(task.Exception);
            }
            returnedValue = m_OldInvoker.InvokeEnd(instance,out outputs,result);
            outputs = outputParams;
            return returnedValue;
         }
         catch
         {
            Debug.Assert(true,"Invoker End exception: " + exception.Message);
            throw exception;
         }
      }
   }
}

#pragma warning restore 618
