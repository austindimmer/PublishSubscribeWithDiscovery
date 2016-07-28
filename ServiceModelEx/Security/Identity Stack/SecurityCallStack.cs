// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http:www.idesign.net

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;

namespace ServiceModelEx
{
   [DataContract]
   public class SecurityCallStack
   {
      [DataMember(IsRequired = true)]
      List<SecurityCallFrame> m_StackFrames = new List<SecurityCallFrame>();

      [MethodImpl(MethodImplOptions.NoInlining)]
      internal void AppendCall()
      {
#if DEBUG
         const int popIndex = 12;
#else
         const int popIndex = 11;
#endif
         AppendCall(popIndex);
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      internal void AppendCall(int index)
      {
         SecurityCallFrame call = new SecurityCallFrame();

         m_StackFrames.Add(call);
         StackFrame frame = new StackFrame(index);
         call.CallerType = frame.GetMethod().DeclaringType.ToString();
         call.Operation = frame.GetMethod().Name + "()";

         if(Count == 1)
         {
            call.Address = Environment.MachineName;
            call.Authentication = Thread.CurrentPrincipal.Identity.AuthenticationType;
            call.ActivityId = Guid.NewGuid();
            call.IdentityName = Thread.CurrentPrincipal.Identity.Name;
            if(call.IdentityName == String.Empty)
            {
               call.IdentityName =WindowsIdentity.GetCurrent().Name;
            }
            call.Operation = frame.GetMethod().Name + "()";
         }
         else //Must be in a service already
         {
            //Add local information for this service
            call.Address = OperationContext.Current.Channel.LocalAddress.Uri.ToString();
            call.Authentication = ServiceSecurityContext.Current.PrimaryIdentity.AuthenticationType;
            call.IdentityName = Thread.CurrentPrincipal.Identity.Name;
            call.ActivityId = m_StackFrames[Count-2].ActivityId;
         }
      }

      internal void Clear()
      {
         m_StackFrames.Clear();
      }

      public SecurityCallFrame OriginalCall
      {
         get
         {
            if(m_StackFrames.Count == 0)
            {
               return null;
            }
            return m_StackFrames[0];
         }
      }

      public SecurityCallFrame LastCall
      {
         get
         {
            if(m_StackFrames.Count == 0)
            {
               return null;
            }
            return m_StackFrames[Count - 1];
         }
      }

      public int Count
      {
         get
         {
            return m_StackFrames.Count;
         }
      }

      public SecurityCallFrame[] Calls
      {
         get
         {
            return m_StackFrames.ToArray();
         }
      }
   }
}
