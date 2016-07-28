// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Collections.Generic;


namespace ServiceModelEx.Transactional
{
   public class TransactionalDictionary<K,T> : TransactionalCollection<Dictionary<K,T>,KeyValuePair<K,T>>,IDictionary<K,T>
   {
      public TransactionalDictionary() : this(0)
      {}
      public TransactionalDictionary(IDictionary<K,T> dictionary) : base(new Dictionary<K,T>(dictionary))
      {}
      public TransactionalDictionary(int capacity) : base(new Dictionary<K,T>(capacity))
      {}
      public int Count
      {
         get
         {
            return Value.Count;
         }
      }
      public bool ContainsKey(K key)
      {
         return Value.ContainsKey(key);
      }
      public ICollection<K> Keys
      {
         get
         {
            return Value.Keys;
         }
      }
      public ICollection<T> Values
      {
         get
         {
            return Value.Values;
         }
      }
      public void Clear()
      {
         Value.Clear();
      }
      void ICollection<KeyValuePair<K,T>>.Add(KeyValuePair<K,T> item)
      {
         (Value as ICollection<KeyValuePair<K,T>>).Add(item);
      }
      public T this[K key]
      {
         get
         {
            return Value[key];
         }
         set
         {
            Value[key] = value;
         }
      }
      public void Add(K key,T item)
      {
         Value.Add(key,item);
      }
      public bool ContainsValue(T item)
      {
         return Value.ContainsValue(item);
      }
      public bool TryGetValue(K key,out T value)
      {
         return Value.TryGetValue(key,out value);
      }
      public bool Remove(K key)
      {
         return Value.Remove(key);
      }
      bool ICollection<KeyValuePair<K,T>>.Contains(KeyValuePair<K,T> item)
      {
         return (Value as ICollection<KeyValuePair<K,T>>).Contains(item);
      }
      void ICollection<KeyValuePair<K,T>>.CopyTo(KeyValuePair<K,T>[] array,int arrayIndex)
      {
         (Value as ICollection<KeyValuePair<K,T>>).CopyTo(array,arrayIndex);
      }
      bool ICollection<KeyValuePair<K,T>>.Remove(KeyValuePair<K,T> item)
      {
         return (Value as ICollection<KeyValuePair<K,T>>).Remove(item);
      }
      bool ICollection<KeyValuePair<K,T>>.IsReadOnly
      {
         get
         {
            return (Value as ICollection<KeyValuePair<K,T>>).IsReadOnly;
         }
      }
      Dictionary<K,T>.Enumerator GetEnumerator()
      {
         return Value.GetEnumerator();
      }
      public IEqualityComparer<K> Comparer
      {
         get
         {
            return Value.Comparer;
         }
      }
   }
}

