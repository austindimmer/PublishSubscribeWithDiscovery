// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net


using System;
using System.Diagnostics;
using ServiceModelEx.Transactional;

namespace ServiceModelEx
{
   public class TransactionalMemoryStore<ID,T> : IInstanceStore<ID,T> where ID : IEquatable<ID>
   {
      static TransactionalDictionary<ID,T> m_Instances = new TransactionalDictionary<ID,T>();

      static TransactionalMemoryStore()
      {
         //Verify [Serializable] on T
         Debug.Assert(typeof(T).IsSerializable);

         //Verify [Serializable] on ID
         Debug.Assert(typeof(ID).IsSerializable);
      }

      public TransactionalMemoryStore()
      {}

      public void RemoveInstance(ID instanceId)
      {
         lock(m_Instances)
         {
            Debug.Assert(ContainsInstance(instanceId));
            m_Instances.Remove(instanceId);
         }
      }
      public bool ContainsInstance(ID instanceId)
      {
         lock(m_Instances)
         {
            return m_Instances.ContainsKey(instanceId);
         }
      }
      public T this[ID instanceId]
      {
         get
         {
            lock(m_Instances)
            {
               return m_Instances[instanceId];
            }
         }
         set
         {
            lock(m_Instances)
            {
               m_Instances[instanceId] = value;
            }
         }
      }
   }
}