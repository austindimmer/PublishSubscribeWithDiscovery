// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ServiceModelEx
{
   public class WcfContextAwaiter : INotifyCompletion
   {
      TaskAwaiter m_Awaiter;
      protected AsyncContext m_AsyncContext;

      protected WcfContextAwaiter()
      {}
      public WcfContextAwaiter(Task task)
      {
         Validate(task);
         m_Awaiter = task.GetAwaiter();
      }

      protected virtual void Validate(Task task)
      {
         if(task == null)
         {
            throw new ArgumentNullException("task");
         }
      }
      public virtual WcfContextAwaiter GetAwaiter()
      {
         return this;
      }
      public virtual bool IsCompleted
      {
         get
         {
            return m_Awaiter.IsCompleted;
         }
      }
      public virtual void OnCompleted(Action continuation)
      {
         m_AsyncContext = new AsyncContext();
         m_Awaiter.OnCompleted(continuation);
      }
      public void GetResult()
      {
         if(m_AsyncContext != null)
         {
            m_AsyncContext.Restore();
         }
         m_Awaiter.GetResult();
      }
   }
   public class WcfContextAwaiter<T> : WcfContextAwaiter
   {
      TaskAwaiter<T> m_Awaiter;

      public WcfContextAwaiter(Task<T> task)
      {
         Validate(task);
         m_Awaiter = task.GetAwaiter();
      }
      public new WcfContextAwaiter<T> GetAwaiter()
      {
         return this;
      }
      public override bool IsCompleted
      {
         get
         {
            return m_Awaiter.IsCompleted;
         }
      }
      public override void OnCompleted(Action continuation)
      {
         m_AsyncContext = new AsyncContext();
         m_Awaiter.OnCompleted(continuation);
      }
      public new T GetResult()
      {
         if(m_AsyncContext != null)
         {
            m_AsyncContext.Restore();
         }
         return m_Awaiter.GetResult();
      }
   }
}
