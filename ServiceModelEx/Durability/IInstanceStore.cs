// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;

namespace ServiceModelEx
{
   public interface IInstanceStore<ID,T> where ID : IEquatable<ID>
   {
      void RemoveInstance(ID instanceId);
      bool ContainsInstance(ID instanceId);
      T this[ID instanceId]
      {
         get;
         set;
      }
   }
}

