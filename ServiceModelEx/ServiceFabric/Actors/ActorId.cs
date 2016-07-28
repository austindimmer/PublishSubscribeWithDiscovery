// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Runtime.Serialization;

namespace ServiceModelEx.ServiceFabric.Actors
{
   [DataContract]
   public enum ActorIdKind
   {
      [EnumMember]
      Long = 0,
      [EnumMember]
      Guid = 1,
      [EnumMember]
      String = 2
   }

   [Serializable]
   [DataContract]
   public class ActorId : IEquatable<ActorId>
   {
      [DataMember]
      string m_ActorId = Guid.Empty.ToString();
      [DataMember]
      ActorIdKind m_Kind = ActorIdKind.Guid;
      [DataMember]
      string m_ApplicationName = string.Empty;
      [DataMember]
      string m_ActorInterfaceName = string.Empty;

      public ActorId(Guid id)
      {
         m_Kind = ActorIdKind.Guid;
         m_ActorId = id.ToString();
      }
      public ActorId(string id)
      {
         m_Kind = ActorIdKind.String;
         m_ActorId = id.ToString();
      }
      public ActorId(long id)
      {
         m_Kind = ActorIdKind.Long;
         m_ActorId = id.ToString();
      }

      public string ApplicationName
      {
         get
         {
            return m_ApplicationName;
         }
         internal set
         {
            m_ApplicationName = value;
         }
      }
      public string ActorInterfaceName
      {
         get
         {
            return m_ActorInterfaceName;
         }
         internal set
         {
            m_ActorInterfaceName = value;
         }
      }

      public ActorIdKind Kind 
      {
         get
         {
            return m_Kind;
         }
      }
      public static ActorId NewId()
      {
         return new ActorId(Guid.NewGuid());
      }
      public bool Equals(ActorId other)
      {
         return m_ActorId.Equals(other.m_ActorId);
      }

      public Guid GetGuidId()
      {
         if(Kind != ActorIdKind.Guid)
         {
            throw new InvalidOperationException("ActorId is not of ActorIdKind Guid.");
         }
         return new Guid(m_ActorId);
      }
      public long GetLongId()
      {
         if(Kind != ActorIdKind.Long)
         {
            throw new InvalidOperationException("ActorId is not of ActorIdKind Long.");
         }
         return Convert.ToInt64(m_ActorId);
      }
      public string GetStringId()
      {
         if(Kind != ActorIdKind.String)
         {
            throw new InvalidOperationException("ActorId is not of ActorIdKind String.");
         }
         return m_ActorId;
      }
      public override int GetHashCode()
      {
         return m_ActorId.GetHashCode();
      }
      public override string ToString()
      {
         return m_ActorId;
      }
   }
}
