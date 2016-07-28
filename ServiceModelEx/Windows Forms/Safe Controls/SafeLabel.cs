// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ServiceModelEx
{
   /// <summary>
   /// Provides thread-safe access to the Text property
   /// </summary>
   [ToolboxBitmap(typeof(SafeLabel),"SafeLabel.bmp")]
   public class SafeLabel : Label
   {
      SynchronizationContext m_SynchronizationContext = SynchronizationContext.Current;
      override public string Text
      {
         set
         {
            try
            {
               m_SynchronizationContext.Send(_=> base.Text = value,null);
            }
            catch
            {}
         }
         get
         {
            string text = String.Empty;
            try
            {
               m_SynchronizationContext.Send(_=> text = base.Text,null);
            }
            catch
            {}
            return text;
         }
      }
   }
}
