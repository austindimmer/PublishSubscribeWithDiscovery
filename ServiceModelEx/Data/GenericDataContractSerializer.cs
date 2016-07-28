// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;



namespace ServiceModelEx
{
   public class DataContractSerializer<T> : XmlObjectSerializer
   {
      DataContractSerializer m_DataContractSerializer;
      
      public DataContractSerializer()
      {
         m_DataContractSerializer = new DataContractSerializer(typeof(T));
      }      
      public DataContractSerializer(IList<Type> knownTypes)
      {
         m_DataContractSerializer = new DataContractSerializer(typeof(T),knownTypes);
      }
      public override object ReadObject(XmlDictionaryReader reader)
      {
         return m_DataContractSerializer.ReadObject(reader);
      }
      public override bool IsStartObject(XmlDictionaryReader reader)
      {
         return m_DataContractSerializer.IsStartObject(reader);
      }
      public override object ReadObject(XmlDictionaryReader reader,bool verifyObjectName)
      {
         return m_DataContractSerializer.ReadObject(reader,verifyObjectName);
      }
      public override void WriteEndObject(XmlDictionaryWriter writer)
      {
         m_DataContractSerializer.WriteEndObject(writer);
      }
      public override void WriteObjectContent(XmlDictionaryWriter writer,object graph)
      {
         m_DataContractSerializer.WriteObjectContent(writer,graph);
      }
      public override void WriteStartObject(XmlDictionaryWriter writer,object graph)
      {
         m_DataContractSerializer.WriteStartObject(writer,graph);
      }
      public new T ReadObject(Stream stream)
      {
         return (T)m_DataContractSerializer.ReadObject(stream);
      }
      public new T ReadObject(XmlReader reader)
      {
         return (T)m_DataContractSerializer.ReadObject(reader);
      }
      public new bool IsStartObject(XmlReader reader)
      {
         return m_DataContractSerializer.IsStartObject(reader);
      }
      public new T ReadObject(XmlReader reader,bool verifyObjectName)
      {
         return (T)m_DataContractSerializer.ReadObject(reader,verifyObjectName);
      }
      public new void WriteEndObject(XmlWriter writer)
      {
         m_DataContractSerializer.WriteEndObject(writer);
      }
      public void WriteObject(Stream stream,T graph)
      {
         m_DataContractSerializer.WriteObject(stream,graph);
      }
      public void WriteObject(XmlDictionaryWriter writer,T graph)
      {
         m_DataContractSerializer.WriteObject(writer,graph);
      }
      public void WriteObject(XmlWriter writer,T graph)
      {
         m_DataContractSerializer.WriteObject(writer,graph);
      }
      public void WriteObjectContent(XmlWriter writer,T graph)
      {
         m_DataContractSerializer.WriteObjectContent(writer,graph);
      }
      public void WriteStartObject(XmlWriter writer,T graph)
      {
         m_DataContractSerializer.WriteStartObject(writer,graph);
      }
   }
}
