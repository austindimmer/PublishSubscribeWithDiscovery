// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceModelEx.ServiceFabric.Services
{
   internal class ServiceChannelInvoker<I> where I : class
   {
      public I Install(ChannelFactory<I> factory)
      {
         CallInvoker invoker = new CallInvoker(typeof(I),factory);
         return invoker.GetTransparentProxy() as I;
      }
      class CallInvoker : RealProxy
      {
         readonly ChannelFactory<I> m_Factory;
         static readonly Assembly[] m_Assemblies;

         static CallInvoker()
         {
            m_Assemblies = AppDomain.CurrentDomain.GetAssemblies();
         }
         public CallInvoker(Type classToProxy,ChannelFactory<I> factory) : base(classToProxy)
         {
            m_Factory = factory;
         }

         IMessage Retry(IMessage message,IMessage response)
         {
            Exception exception = null;
            Task result = (response as IMethodReturnMessage).ReturnValue as Task;
            if(result != null)
            {
               exception = result.Exception.InnerException;
            }
            else
            {
               exception = (response as IMethodReturnMessage).Exception;
            }

            //Only retry on transient timeout exceptions.
            if(!(exception is TimeoutException))
            {
               return response;
            }

            int retryCount = (int)message.Properties["retryCount"];
            retryCount++;
            if(retryCount < 5)
            {
               message.Properties["retryCount"] = retryCount;
               Thread.Sleep(5000 * retryCount);
               return Invoke(message);
            }
            return response;
         }
         Type FindExceptionType(string typeName)
         {
            Type type = null;
            try
            {
               IEnumerable<Assembly> assemblies = m_Assemblies.Where(assembly=>assembly.GetType(typeName) != null);
               type = assemblies.First().GetType(typeName);
               Debug.Assert(type != null,"Make sure this assembly (ServiceModelEx by default) contains the definition of the custom exception");
               Debug.Assert(type.IsSubclassOf(typeof(Exception)));
            }
            catch
            {
               type = typeof(Exception);
            }

            return type;
         }
         AggregateException ExtractException(ExceptionDetail detail)
         {
            AggregateException innerException = null;
            if(detail.InnerException != null)
            {
               innerException = ExtractException(detail.InnerException);
            }

            Type type = FindExceptionType(detail.Type);
            Type[] parameters = { typeof(string),typeof(Exception) };
            ConstructorInfo info = type.GetConstructor(parameters);
            Debug.Assert(info != null,"Exception type " + detail.Type + " does not have suitable constructor");

            AggregateException exception = new AggregateException(Activator.CreateInstance(type,detail.Message,innerException) as Exception);
            Debug.Assert(exception != null);
            return exception;
         }
         Exception EvaluateException(Exception rootException)
         {
            Exception exception = rootException;
            Exception innerException = rootException.InnerException;

            //Since we're invoking, the outer exception will always be an invocation exception.
            if(innerException == null)
            {
               exception = rootException;
            }
            else if(!(innerException is FaultException))
            {
               exception = innerException;
            }
            else
            {
               if(!(innerException is FaultException<ExceptionDetail>))
               {
                  exception = innerException;
               }
               else
               {
                  exception = ExtractException((innerException as FaultException<ExceptionDetail>).Detail);
               }
            }
            return exception;
         }

         async Task<IMessage> InvokeAsync(IClientChannel channel,MethodCallMessageWrapper methodCallWrapper)
         {
            ReturnMessage response = await (methodCallWrapper.MethodBase.Invoke(channel,methodCallWrapper.Args) as Task).ContinueWith(result => new ReturnMessage(result,null,0,methodCallWrapper.LogicalCallContext,methodCallWrapper));
            return response;
         }
         public override IMessage Invoke(IMessage message)
         {
            MethodCallMessageWrapper methodCallWrapper = new MethodCallMessageWrapper((IMethodCallMessage)message);

            if(!message.Properties.Contains("retryCount"))
            {
               message.Properties.Add("retryCount",0);
            }

            IMessage response = null;
            IClientChannel channel = null;
            try
            {
               channel = m_Factory.CreateChannel() as IClientChannel;
               channel.OperationTimeout = TimeSpan.MaxValue;
               channel.Open();

               response = InvokeAsync(channel,methodCallWrapper).Result;

               Task result = ((response as IMethodReturnMessage).ReturnValue as Task);
               if(result.IsFaulted)
               {
                  response = new ReturnMessage(EvaluateException(result.Exception),methodCallWrapper);
                  response = Retry(message,response);
               }
               return response;
            }
            catch(TargetInvocationException exception)
            {
               if(response == null)
               {
                  response = new ReturnMessage(exception.InnerException,methodCallWrapper);
               }
               return Retry(message,response);
               throw exception.InnerException;
            }
            catch(TimeoutException exception)
            {
               if(response == null)
               {
                  response = new ReturnMessage(exception,methodCallWrapper);
               }
               return Retry(message,response);
               throw;
            }
            finally
            {
               if(channel.State != CommunicationState.Closed && channel.State != CommunicationState.Faulted)
               {
                  try
                  {
                     channel.Close();
                  }
                  catch
                  {
                     channel.Abort();
                  }
               }
               channel = null;
            }
         }
      }
   }
}
