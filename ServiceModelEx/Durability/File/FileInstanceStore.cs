// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServiceModelEx
{
   public class FileInstanceStore<ID,T> : IInstanceStore<ID,T> where ID : IEquatable<ID>
   {
      protected readonly string Filename;

      static FileInstanceStore()
      {
         //Verify [Serializable] on T
         Debug.Assert(typeof(T).IsSerializable);

         //Verify [Serializable] on ID
         Debug.Assert(typeof(ID).IsSerializable);
      }

      public FileInstanceStore(string fileName)
      {
         Filename = fileName;

         //Initialize the file for the first time only
         using(Stream stream = new FileStream(Filename,FileMode.OpenOrCreate))
         {
            if(stream.Length == 0)
            {
               IFormatter formatter = new BinaryFormatter();
               formatter.Serialize(stream,new Dictionary<ID,T>());
               stream.Flush();
            }
         }
      }

      public void RemoveInstance(ID instanceId)
      {
         using(Stream stream = new FileStream(Filename,FileMode.OpenOrCreate))
         {
            IFormatter formatter = new BinaryFormatter();
            Dictionary<ID,T> instances = formatter.Deserialize(stream) as Dictionary<ID,T>;
            Debug.Assert(instances != null);
            instances.Remove(instanceId);

            stream.Seek(0,SeekOrigin.Begin);
            formatter.Serialize(stream,instances);
            stream.Flush();
         }
      }
      public bool ContainsInstance(ID instanceId)
      {
         using(Stream stream = new FileStream(Filename,FileMode.OpenOrCreate))
         {
            IFormatter formatter = new BinaryFormatter();
            Dictionary<ID,T> calculators = formatter.Deserialize(stream) as Dictionary<ID,T>;
            Debug.Assert(calculators != null);
            return calculators.ContainsKey(instanceId);
         }
      }
      public T this[ID instanceId]
      {
         get
         {
            using(Stream stream = new FileStream(Filename,FileMode.OpenOrCreate))
            {
               IFormatter formatter = new BinaryFormatter();
               Dictionary<ID,T> instances = formatter.Deserialize(stream) as Dictionary<ID,T>;
               Debug.Assert(instances != null);
               return instances[instanceId];
            }
         }
         set
         {
            using(Stream stream = new FileStream(Filename,FileMode.OpenOrCreate))
            {
               IFormatter formatter = new BinaryFormatter();
               Dictionary<ID,T> instances = formatter.Deserialize(stream) as Dictionary<ID,T>;
               Debug.Assert(instances != null);

               instances[instanceId] = value;
               stream.Seek(0,SeekOrigin.Begin);
               formatter.Serialize(stream,instances);

               stream.Flush();
            }
         }
      }
   }
}