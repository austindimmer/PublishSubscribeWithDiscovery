// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Threading;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;

namespace ServiceModelEx
{
   [DataContract(Name = "LogbookEntry")]
   public struct LogbookEntryClient
   {
      public LogbookEntryClient(string assemblyName,string fileName,int lineNumber,string typeName,string methodName,string exceptionName,string exceptionMessage,string providedFault,string providedMessage,string eventDescription) : this(assemblyName,fileName,lineNumber,typeName,methodName,exceptionName,exceptionMessage,providedFault,providedMessage)
      {
         Event = eventDescription;
      }
      public LogbookEntryClient(string assemblyName,string fileName,int lineNumber,string typeName,string methodName,string exceptionName,string exceptionMessage) : this(assemblyName,fileName,lineNumber,typeName,methodName,exceptionName,exceptionMessage,String.Empty,String.Empty)
      {}
      public LogbookEntryClient(string assemblyName,string fileName,int lineNumber,string typeName,string methodName,string exceptionName,string exceptionMessage,string providedFault,string providedMessage)
      {
         MachineName = Environment.MachineName;
         Assembly entryAssembly = Assembly.GetEntryAssembly();
         if(entryAssembly == null)
         {
            HostName = Process.GetCurrentProcess().MainModule.ModuleName;
         }
         else
         {
            HostName = entryAssembly.GetName().Name;
         }
         AssemblyName = assemblyName;
         FileName = fileName;
         LineNumber = lineNumber;
         TypeName = typeName;
         MemberAccessed = methodName;
         Date = DateTime.Now.ToShortDateString();
         Time = DateTime.Now.ToLongTimeString();
         ExceptionName = exceptionName;
         ExceptionMessage = exceptionMessage;
         ProvidedFault = providedFault;
         ProvidedMessage = providedMessage;
         Event = String.Empty;
      }
      [DataMember]
      public readonly string MachineName;

      [DataMember]
      public readonly string HostName;

      [DataMember]
      public readonly string AssemblyName;

      [DataMember]
      public readonly string FileName;

      [DataMember]
      public readonly int LineNumber;

      [DataMember]
      public readonly string TypeName;

      [DataMember]
      public readonly string MemberAccessed;

      [DataMember]
      public readonly string Date;

      [DataMember]
      public readonly string Time;

      [DataMember]
      public readonly string ExceptionName;

      [DataMember]
      public readonly string ExceptionMessage;

      [DataMember]
      public readonly string ProvidedFault;

      [DataMember]
      public readonly string ProvidedMessage;

      [DataMember]
      public readonly string Event;
   }
}