// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections;
using System.Collections.Generic;


namespace ServiceModelEx.Transactional
{
   public class TransactionalSortedDictionary<K,T> : TransactionalCollection<SortedDictionary<K,T>,KeyValuePair<K,T>>,IDictionary<K,T>,IDictionary
   {
      public TransactionalSortedDictionary(IDictionary<K,T> dictionary) : base(new SortedDictionary<K,T>(dictionary))
      {}
      public TransactionalSortedDictionary(IDictionary<K,T> dictionary,IComparer<K> comparer) : base(new SortedDictionary<K,T>(dictionary,comparer))
      {}
      public TransactionalSortedDictionary(IComparer<K> comparer) : base(new SortedDictionary<K,T>(comparer))
      {}
      public int Count
      {
         get
         {
            return Value.Count;
         }
      }
      public IComparer<K> Comparer
      {
         get
         {
            return Value.Comparer;
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
      object ICollection.SyncRoot
      {
         get
         {
            return (Value as ICollection).SyncRoot;
         }
      }
      bool ICollection.IsSynchronized
      {
         get
         {
            return (Value as ICollection).IsSynchronized;
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
      void IDictionary.Remove(object key)
      {
         (Value as IDictionary<K,T>).Remove((K)key);
      }
      bool ICollection<KeyValuePair<K,T>>.Contains(KeyValuePair<K,T> item)
      {
         return (Value as ICollection<KeyValuePair<K,T>>).Contains(item);
      }
      void CopyTo(KeyValuePair<K,T>[] array,int arrayIndex)
      {
         Value.CopyTo(array,arrayIndex);
      }
      void ICollection<KeyValuePair<K,T>>.CopyTo(KeyValuePair<K,T>[] array,int arrayIndex)
      {
         (Value as ICollection<KeyValuePair<K,T>>).CopyTo(array,arrayIndex);
      }
      void ICollection.CopyTo(Array array,int arrayIndex)
      {
         (Value as ICollection).CopyTo(array,arrayIndex);
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
      bool IDictionary.IsReadOnly
      {
         get
         {
            return (Value as IDictionary).IsReadOnly;
         }
      }
      void IDictionary.Add(object key,object value)
      {
         (Value as IDictionary<K,T>).Add((K)key,(T)value);
      }
      bool IDictionary.Contains(object key)
      {
         return (Value as IDictionary<K,T>).ContainsKey((K)key);
      }

      bool IDictionary.IsFixedSize
      {
         get
         {
            return (Value as IDictionary).IsFixedSize;
         }
      }
      object IDictionary.this[object key]
      {
         get
         {
            return (Value as IDictionary)[(K)key];
         }
         set
         {
            (Value as SortedDictionary<K,T>)[(K)key] = (T)value;
         }
      }
      ICollection IDictionary.Keys
      {
         get
         {
            return (Value as IDictionary).Keys;
         }
      }
      ICollection IDictionary.Values
      {
         get
         {
            return (Value as IDictionary).Values;
         }
      }
      IDictionaryEnumerator IDictionary.GetEnumerator()
      {
         return (Value as IDictionary).GetEnumerator();
      }
      SortedDictionary<K,T>.Enumerator GetEnumerator()
      {
         return Value.GetEnumerator();
      }
   }
}

