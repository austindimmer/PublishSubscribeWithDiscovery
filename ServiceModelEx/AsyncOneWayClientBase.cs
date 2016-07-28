// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using System.Threading;

namespace ServiceModelEx
{
   public class AsyncOneWayClientBase<T> : ClientBase<T> where T : class
   {
      List<WaitHandle> m_PendingOperations = new List<WaitHandle>();

      static AsyncOneWayClientBase()
      {
         Type type = typeof(T);
         Debug.Assert(type.IsInterface);

         MethodInfo[] methods = type.GetMethods();

         foreach(MethodInfo method in methods)
         {
            object[] attributes = method.GetCustomAttributes(typeof(OperationContractAttribute),true);

            if(attributes.Length == 0)
            {
               Debug.Assert(method.Name.StartsWith("End"));
               Debug.Assert(method.ReturnType == typeof(void));

               ParameterInfo[] parameters = method.GetParameters();
               Debug.Assert(parameters[parameters.Length-1].ParameterType == typeof(IAsyncResult));

               continue;
            }
            OperationContractAttribute operationContract = attributes[0] as OperationContractAttribute;


            if(operationContract.IsOneWay == false)
            {
               throw new InvalidOperationException("All operations on contract " + type + " must be one-way, but operation " + method.Name + " is not configured for one-way");
            }
         }
      }
      protected AsyncCallback GetCompletion()
      {
         ManualResetEvent handle = new ManualResetEvent(false);
         lock(m_PendingOperations)
         {
            m_PendingOperations.Add(handle);
         }
         return _=>
                {
                   handle.Set();                   
                   lock(m_PendingOperations)
                   {

                      m_PendingOperations.Remove(handle);
                   }
                };
      }
      public new void Close()
      {
         lock(m_PendingOperations)
         {
            WaitHandle[] operations = m_PendingOperations.ToArray();
            if(operations.Length > 0)
            {
               WaitHandle.WaitAll(operations,Endpoint.Binding.SendTimeout);
            }
         }
         base.Close();
      }
      public void Dispose()
      {
         Close();
      }
   }
}