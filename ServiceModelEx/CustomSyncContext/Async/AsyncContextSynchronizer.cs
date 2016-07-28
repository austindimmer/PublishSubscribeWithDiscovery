// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.ServiceModel;
using System.Threading;

namespace ServiceModelEx
{
   [SecurityPermission(SecurityAction.Demand,ControlThread = true)]
   public class AsyncContextSynchronizer : ThreadPoolSynchronizer
   {
      class AsyncContextWorkerThread : ThreadPoolSynchronizer.WorkerThread
      {
         internal AsyncContextWorkerThread(string name,ThreadPoolSynchronizer context) : base(name,context)
         {}

         ExecutionContext RetrieveContinuationExecutionContext(WorkItem workItem)
         {
            object target = workItem.State.GetType().InvokeMember("Target",
                                                                  System.Reflection.BindingFlags.NonPublic |
                                                                  System.Reflection.BindingFlags.Public |
                                                                  System.Reflection.BindingFlags.Instance |
                                                                  System.Reflection.BindingFlags.GetProperty,
                                                                  null,workItem.State,new object[0]);

            ExecutionContext executionContext = target.GetType().InvokeMember("m_context",
                                                                              System.Reflection.BindingFlags.NonPublic |
                                                                              System.Reflection.BindingFlags.Public |
                                                                              System.Reflection.BindingFlags.Instance |
                                                                              System.Reflection.BindingFlags.GetField,
                                                                              null,target,new object[0]) as ExecutionContext;

            return executionContext;
         }
         ExecutionContext RetrieveContinuationExecutionContextInDebug(WorkItem workItem)
         {
            //EC: If Invoke(), State.Target.m_continuation.Target.m_continuation.Target.m_context
            //EC: If Run(), State.Target.m_continuation.Target.m_context
            MemberInfo[] infos = workItem.State.GetType().GetMembers(System.Reflection.BindingFlags.NonPublic |
                                                                     System.Reflection.BindingFlags.Public |
                                                                     System.Reflection.BindingFlags.Instance);

            object target = workItem.State.GetType().InvokeMember("Target",
                                                                  System.Reflection.BindingFlags.NonPublic |
                                                                  System.Reflection.BindingFlags.Public |
                                                                  System.Reflection.BindingFlags.Instance |
                                                                  System.Reflection.BindingFlags.GetProperty,
                                                                  null,workItem.State,new object[0]);

            infos = target.GetType().GetMembers(System.Reflection.BindingFlags.NonPublic |
                                                System.Reflection.BindingFlags.Public |
                                                System.Reflection.BindingFlags.Instance);

            object continuation = target.GetType().InvokeMember("m_continuation",
                                                                System.Reflection.BindingFlags.NonPublic |
                                                                System.Reflection.BindingFlags.Public |
                                                                System.Reflection.BindingFlags.Instance |
                                                                System.Reflection.BindingFlags.GetField,
                                                                null,target,new object[0]);

            infos = continuation.GetType().GetMembers(System.Reflection.BindingFlags.NonPublic |
                                                System.Reflection.BindingFlags.Public |
                                                System.Reflection.BindingFlags.Instance);

            object runTarget = null;
            if((continuation as Action).Method.Name.Contains("Invoke"))
            {
               object continuationTarget = continuation.GetType().InvokeMember("Target",
                                                                          System.Reflection.BindingFlags.NonPublic |
                                                                          System.Reflection.BindingFlags.Public |
                                                                          System.Reflection.BindingFlags.Instance |
                                                                          System.Reflection.BindingFlags.GetProperty,
                                                                          null,continuation,new object[0]);

               infos = continuationTarget.GetType().GetMembers(System.Reflection.BindingFlags.NonPublic |
                                                   System.Reflection.BindingFlags.Public |
                                                   System.Reflection.BindingFlags.Instance);

               runTarget = continuation.GetType().InvokeMember("Target",
                                                                          System.Reflection.BindingFlags.NonPublic |
                                                                          System.Reflection.BindingFlags.Public |
                                                                          System.Reflection.BindingFlags.Instance |
                                                                          System.Reflection.BindingFlags.GetProperty,
                                                                          null,continuationTarget,new object[0]);
            }
            else
            {
               runTarget = continuation.GetType().InvokeMember("Target",
                                                                          System.Reflection.BindingFlags.NonPublic |
                                                                          System.Reflection.BindingFlags.Public |
                                                                          System.Reflection.BindingFlags.Instance |
                                                                          System.Reflection.BindingFlags.GetProperty,
                                                                          null,continuation,new object[0]);
            }

            infos = runTarget.GetType().GetMembers(System.Reflection.BindingFlags.NonPublic |
                                                System.Reflection.BindingFlags.Public |
                                                System.Reflection.BindingFlags.Instance);

            ExecutionContext executionContext = runTarget.GetType().InvokeMember("m_context",
                                                                                 System.Reflection.BindingFlags.NonPublic |
                                                                                 System.Reflection.BindingFlags.Public |
                                                                                 System.Reflection.BindingFlags.Instance |
                                                                                 System.Reflection.BindingFlags.GetField,
                                                                                 null,runTarget,new object[0]) as ExecutionContext;

            return executionContext;
         }
         //Only works when debugger is attached in VS 2015
         OperationContext RetrieveContinuationOperationContext(WorkItem workItem)
         {
            //Note: As of VS 2015, State.Target.m_invokeAction.Target.task.AsyncState is OperationContext!!!!
            MemberInfo[] infos = workItem.State.GetType().GetMembers(System.Reflection.BindingFlags.NonPublic |
                                                                     System.Reflection.BindingFlags.Public |
                                                                     System.Reflection.BindingFlags.Instance);

            object target = workItem.State.GetType().InvokeMember("Target",
                                                                  System.Reflection.BindingFlags.NonPublic |
                                                                  System.Reflection.BindingFlags.Public |
                                                                  System.Reflection.BindingFlags.Instance |
                                                                  System.Reflection.BindingFlags.GetProperty,
                                                                  null,workItem.State,new object[0]);

            infos = target.GetType().GetMembers(System.Reflection.BindingFlags.NonPublic |
                                                System.Reflection.BindingFlags.Public |
                                                System.Reflection.BindingFlags.Instance);

            object invokeAction = target.GetType().InvokeMember("m_invokeAction",
                                                                System.Reflection.BindingFlags.NonPublic |
                                                                System.Reflection.BindingFlags.Public |
                                                                System.Reflection.BindingFlags.Instance |
                                                                System.Reflection.BindingFlags.GetField,
                                                                null,target,new object[0]);

            infos = invokeAction.GetType().GetMembers(System.Reflection.BindingFlags.NonPublic |
                                                      System.Reflection.BindingFlags.Public |
                                                      System.Reflection.BindingFlags.Instance);

            object invokeTarget = invokeAction.GetType().InvokeMember("Target",
                                                                      System.Reflection.BindingFlags.NonPublic |
                                                                      System.Reflection.BindingFlags.Public |
                                                                      System.Reflection.BindingFlags.Instance |
                                                                      System.Reflection.BindingFlags.GetProperty,
                                                                      null,invokeAction,new object[0]);

            infos = invokeTarget.GetType().GetMembers(System.Reflection.BindingFlags.NonPublic |
                                                      System.Reflection.BindingFlags.Public |
                                                      System.Reflection.BindingFlags.Instance);

            object invokeTargetTask = invokeTarget.GetType().InvokeMember("task",
                                                                          System.Reflection.BindingFlags.NonPublic |
                                                                          System.Reflection.BindingFlags.Public |
                                                                          System.Reflection.BindingFlags.Instance |
                                                                          System.Reflection.BindingFlags.GetField,
                                                                          null,invokeTarget,new object[0]);

            infos = invokeTargetTask.GetType().GetMembers(System.Reflection.BindingFlags.NonPublic |
                                                          System.Reflection.BindingFlags.Public |
                                                          System.Reflection.BindingFlags.Instance);

            OperationContext context = invokeTargetTask.GetType().InvokeMember("AsyncState",
                                                                          System.Reflection.BindingFlags.NonPublic |
                                                                          System.Reflection.BindingFlags.Public |
                                                                          System.Reflection.BindingFlags.Instance |
                                                                          System.Reflection.BindingFlags.GetProperty,
                                                                          null,invokeTargetTask,new object[0]) as OperationContext;

            return context;
         }
         internal override void ProcessItem(WorkItem workItem)
         {
            try
            {
               if((workItem.State is Delegate) == false)
               {
                  CallContext.LogicalSetData(typeof(AsyncContext).Name,workItem.AsyncContext);
                  workItem.CallBack();
               }
               else
               {
                  ExecutionContext executionContext = null;
                  if(Debugger.IsAttached)
                  {
                     executionContext = RetrieveContinuationExecutionContextInDebug(workItem);
                  }
                  else
                  {
                     executionContext = RetrieveContinuationExecutionContext(workItem);
                  }
                  ExecutionContext.Run(executionContext,
                                       delegate
                                       {
                                          AsyncContext asyncContext = CallContext.LogicalGetData(typeof(AsyncContext).Name) as AsyncContext;
                                          if(asyncContext != null)
                                          {
                                             asyncContext.Restore(RestoreFlags.Transaction | RestoreFlags.OperationContext);
                                          }
                                          workItem.CallBack();
                                       },
                                       null);
               }
            }
            catch (Exception exception)
            {
               Debug.Assert(false,"AsyncContextSynchronizer failed! " + exception.Message);
            }
         }
      }

      protected override void InitializeThreads(uint poolSize,string poolName)
      {
         m_WorkerThreads = new AsyncContextWorkerThread[poolSize];
         for (int index = 0; index < poolSize; index++)
         {
            m_WorkerThreads[index] = new AsyncContextWorkerThread(poolName + " " + (index + 1),this);
         }
      }

      public AsyncContextSynchronizer(uint poolSize) : this(poolSize,"Async Context Pool Thread: ")
      {}
      public AsyncContextSynchronizer(uint poolSize,string poolName) : base(poolSize,poolName)
      {}
      public override void Post(SendOrPostCallback method,object state)
      {
         WorkItem workItem = new WorkItem(method,state,new AsyncContext());
         QueueWorkItem(workItem);
      }
   }
}