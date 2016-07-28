// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
using System.Threading;

namespace ServiceModelEx
{
   public abstract class FilePublishService<T> :  PublishService<T> where T : class
   {
      protected override void FireEvent(params object[] args)
      {
         string action = OperationContext.Current.IncomingMessageHeaders.Action;
         string[] slashes = action.Split('/');
         string methodName = slashes[slashes.Length-1];

         FireEvent(methodName,args);
      }
      static void FireEvent(string methodName,object[] args)
      {
         PublishPersistent(methodName,args);
         PublishTransient(methodName,args);
      }
      static void PublishPersistent(string methodName,object[] args)
      {
         T[] subscribers = FileSubscriptionManager<T>.GetFilePersistentList(methodName);
         Publish(subscribers,true,methodName,args);
      }
   }
}