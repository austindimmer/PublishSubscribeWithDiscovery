// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceModelEx
{
   public class OneWayClientBaseAsync<T> : ClientBase<T> where T : class
   {
      List<Task> m_PendingOperations = new List<Task>();

      static OneWayClientBaseAsync()
      {
         Type type = typeof(T);
         Debug.Assert(type.IsInterface);

         MethodInfo[] methods = type.GetMethods();

         foreach(MethodInfo method in methods)
         {
            object[] attributes = method.GetCustomAttributes(typeof(OperationContractAttribute),true);

            if(attributes.Length > 0)
            {
               OperationContractAttribute operationContract = attributes[0] as OperationContractAttribute;
               if(operationContract.IsOneWay == false)
               {
                  throw new InvalidOperationException("All operations on contract " + type + " must be one-way, but operation " + method.Name + " is not configured for one-way");
               }
               else
               {
                  if(method.Name.EndsWith("Async"))
                  {
                     Debug.Assert(method.ReturnType == typeof(Task));
                  }
               }
            }
         }
      }

      protected async Task Invoke(Task task)
      {
         lock(m_PendingOperations)
         {
            m_PendingOperations.Add(task);
         }
         try
         {
             await task.ConfigureAwait(false);
         }
         catch
         {
            WaitHandle handle = (task as IAsyncResult).AsyncWaitHandle;
            (task as IAsyncResult).AsyncWaitHandle.Dispose();
         }
         finally
         {
            ((IAsyncResult)task).AsyncWaitHandle.Dispose();
            lock(m_PendingOperations)
            {
               m_PendingOperations.Remove(task);
            }
         }
      }
      public new void Close()
      {
         lock(m_PendingOperations)
         {
            Task[] tasks = m_PendingOperations.ToArray();
            Task.WaitAll(tasks);
         }

         base.Close();
      }
      public void Dispose()
      {
         Close();
      }
   }
}