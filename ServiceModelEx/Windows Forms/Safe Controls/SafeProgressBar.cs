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
	/// Provides thread-safe access to some methods and properties
	/// </summary>
   [ToolboxBitmap(typeof(SafeProgressBar),"SafeProgressBar.bmp")]
   public class SafeProgressBar : ProgressBar
   {
      SynchronizationContext m_SynchronizationContext = SynchronizationContext.Current;

      public int GetValue()
      {
         int value = 0;
         SendOrPostCallback getValue = delegate
                                       {
                                          value = base.Value;
                                       };
         try
         {
            m_SynchronizationContext.Send(getValue,null);
         }
         catch
         {}
         return value;   
      }
      public void SetValue(int progress)
      {
         SendOrPostCallback setValue = delegate(object value)
                                       {
                                          base.Value = (int)value;
                                       };
         try
         {
            m_SynchronizationContext.Send(setValue,progress);
         }
         catch
         {}
      }
   }
}
