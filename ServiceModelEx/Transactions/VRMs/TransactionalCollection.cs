// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Collections;
using System.Collections.Generic;


namespace ServiceModelEx.Transactional
{
   public abstract class TransactionalCollection<C,T> : Transactional<C>,IEnumerable<T> where C : IEnumerable<T>
   {
      public TransactionalCollection(C collection)
      {
         Value = collection;
      }
      IEnumerator<T> IEnumerable<T>.GetEnumerator()
      {
         return Value.GetEnumerator();
      }
      IEnumerator IEnumerable.GetEnumerator()
      {
         IEnumerable<T> enumerable = this;
         return enumerable.GetEnumerator();
      }
   }
}