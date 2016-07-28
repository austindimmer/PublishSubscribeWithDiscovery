// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace ServiceModelEx.ServiceFabric.Actors
{
   [AttributeUsage(AttributeTargets.Class)]
   internal class ActorErrorHandlerBehaviorAttribute : Attribute,IErrorHandler,IServiceBehavior
   {
      void IServiceBehavior.Validate(ServiceDescription description,ServiceHostBase host)
      {}
      void IServiceBehavior.AddBindingParameters(ServiceDescription description,ServiceHostBase host,Collection<ServiceEndpoint> endpoints,BindingParameterCollection parameters)
      {}
      void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description,ServiceHostBase host)
      {
         foreach(ChannelDispatcher dispatcher in host.ChannelDispatchers)
         {
            dispatcher.ErrorHandlers.Add(this);
         }
      }

      private Type ExceptionType(Exception error,int paramCount)
      {
         Type errorType = error.GetType();
         if(!errorType.GetConstructors().Any(constructor =>
                                             {
                                                ParameterInfo[] info = constructor.GetParameters();
                                                if((info.Count() == paramCount) && (info[0].ParameterType == typeof(string)))
                                                {
                                                   return true;
                                                }
                                                else
                                                {
                                                   return false;
                                                }
                                             }))
            errorType = typeof(Exception);

         return errorType;
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

      public virtual bool HandleError(Exception error)
      {
         return false;
      }
      void IErrorHandler.ProvideFault(Exception error,MessageVersion version,ref Message fault)
      {
         if(error.GetType().IsGenericType && error is FaultException)
         {
            Debug.Assert(error.GetType().GetGenericTypeDefinition() == typeof(FaultException<>));
            return;
         }

         try
         {
            Type detailType = null;
            object detail = null;

            //Always return details. The channel invoker translates them into an exception hierarchy.
            ExceptionDetail newDetail = new ExceptionDetail(error);
            detailType = newDetail.GetType();
            detail = newDetail;

            Type faultUnboundedType = typeof(FaultException<>);
            Type faultBoundedType = faultUnboundedType.MakeGenericType(detailType);
            FaultException faultException = (FaultException)Activator.CreateInstance(faultBoundedType,detail,error.Message);
            MessageFault messageFault = faultException.CreateMessageFault();
            fault = Message.CreateMessage(version,messageFault,faultException.Action);
         }
         catch
         {}
      }
   }
}





