// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections;
using System.Collections.Generic;


namespace ServiceModelEx.Transactional
{
   public class TransactionalLinkedList<T> : TransactionalCollection<LinkedList<T>,T>,ICollection<T>,IEnumerable<T>,ICollection
   {
      public TransactionalLinkedList(IEnumerable<T> collection) : base(new LinkedList<T>(collection))
      {}
      public int Count
      {
         get
         {
            return Value.Count;
         }
      }
      void ICollection<T>.Add(T item)
      {
         (Value as ICollection<T>).Add(item);
      }
      public void Clear()
      {
         Value.Clear();
      }
      public bool Contains(T item)
      {
         return Value.Contains(item);
      }
      public bool Remove(T item)
      {
         return Value.Remove(item);
      }      
      public void CopyTo(T[] array,int arrayIndex)
      {
         Value.CopyTo(array,arrayIndex);
      }
      bool ICollection<T>.IsReadOnly
      {
         get
         {
            return (Value as ICollection<T>).IsReadOnly;
         }
      }
      void ICollection.CopyTo(Array array,int arrayIndex)
      {
         (Value as ICollection).CopyTo(array,arrayIndex);
      }
      public bool IsSynchronized
      {
         get
         {
            return false;
         }
      }
      public object SyncRoot
      {
         get
         {
            return this;
         }
      }
      public LinkedListNode<T> AddAfter(LinkedListNode<T> node,T value)
      {
         return Value.AddAfter(node,value);
      }
      public LinkedListNode<T> AddBefore(LinkedListNode<T> node,T value)
      {
         return Value.AddBefore(node,value);
      }
      public void AddFirst(LinkedListNode<T> node)
      {
         Value.AddFirst(node);
      }
      public LinkedListNode<T> AddFirst(T value)
      {
         return Value.AddFirst(value);
      }
      public LinkedListNode<T> AddLast(T value)
      {
         return Value.AddLast(value);
      }
      public void AddLast(LinkedListNode<T> node)
      {
         Value.AddLast(node);
      }
      public LinkedListNode<T> Find(T value)
      {
         return Value.Find(value);
      }
      public LinkedListNode<T> FindLast(T value)
      {
         return Value.FindLast(value);
      }
      public void Remove(LinkedListNode<T> node)
      {
         Value.Remove(node);
      }
      public void RemoveFirst()
      {
         Value.RemoveFirst();
      }
      public void RemoveLast()
      {
         Value.RemoveLast();
      }
      public LinkedListNode<T> First
      {
         get
         {
            return Value.First;
         }
      }
      public LinkedListNode<T> Last
      {
         get
         {
            return Value.Last;
         }
      }
   }
}

