// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ServiceModelEx
{
   /// <summary>
	/// Provides thread-safe enabling of the button
	/// </summary>
   [ToolboxBitmap(typeof(SafeButton),"SafeButton.bmp")]
   public class SafeButton : Button
   {
      SynchronizationContext m_SynchronizationContext = SynchronizationContext.Current;
      
      public bool SafeEnabled
      {
         set
         {
            SendOrPostCallback enable = delegate(object enabled)
                                        {
                                           base.Enabled = (bool)enabled;
                                        };
            try
            {
               m_SynchronizationContext.Send(enable,value);
            }
            catch
            {}
         }
         get
         {
            bool status = false;
            SendOrPostCallback enabled = delegate
                                         {
                                            status = base.Enabled;
                                         };
            try
            {
               m_SynchronizationContext.Send(enabled,null);
            }
            catch
            {}
            
            return status;
         }
      }
   }
}
