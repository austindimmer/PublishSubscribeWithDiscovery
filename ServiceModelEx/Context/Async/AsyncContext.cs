// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.ServiceModel;
using System.Threading;
using System.Transactions;

namespace ServiceModelEx
{
   public enum RestoreFlags
   {
      Transaction             = 0x01,
      OperationContext        = 0x02,
      SynchronizationContext  = 0x04
   }
   public class AsyncContext
   {
      public readonly OperationContext OperationContext;
      public readonly Transaction Transaction;
      public readonly SynchronizationContext SynchronizationContext;

      public AsyncContext() : this(OperationContext.Current,Transaction.Current,SynchronizationContext.Current)
      {}
      public AsyncContext(AsyncContext asyncContext) : this(asyncContext.OperationContext,asyncContext.Transaction,asyncContext.SynchronizationContext)
      {}
      public AsyncContext(OperationContext operationContext,Transaction transaction,SynchronizationContext synchronizationContext)
      {
         OperationContext = operationContext;
         Transaction = transaction;
         SynchronizationContext = synchronizationContext;
      }

      public void Restore(RestoreFlags restoreFlags = RestoreFlags.Transaction | RestoreFlags.OperationContext | RestoreFlags.SynchronizationContext)
      {
         if((restoreFlags & RestoreFlags.Transaction) == RestoreFlags.Transaction)
         {
            Transaction.Current = Transaction;
         }
         if((restoreFlags & RestoreFlags.OperationContext) == RestoreFlags.OperationContext)
         {
            OperationContext.Current = OperationContext;
         }
         if((restoreFlags & RestoreFlags.SynchronizationContext) == RestoreFlags.SynchronizationContext)
         {
            SynchronizationContext.SetSynchronizationContext(SynchronizationContext);
         }
      }
   }
}
