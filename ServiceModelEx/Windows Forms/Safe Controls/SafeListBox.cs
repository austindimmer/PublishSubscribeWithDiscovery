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
   /// Provides thread-safe access way to add items
   /// </summary>
   [ToolboxBitmap(typeof(SafeListBox),"SafeListBox.bmp")]
   public class SafeListBox : ListBox
   {
      SynchronizationContext m_SynchronizationContext = SynchronizationContext.Current;

      override public string Text
      {
         set
         {
            SendOrPostCallback setText = delegate(object text)
                                         {
                                            base.Text = text as string;
                                         };
            try
            {
               m_SynchronizationContext.Send(setText,value);
            }
            catch
            {}
         }
         get
         {
            string text = String.Empty;
            SendOrPostCallback getText = delegate
                                         {
                                            text = base.Text;
                                         };
            try
            {
               m_SynchronizationContext.Send(getText,null);

            }
            catch
            {}
            return text;
         }
      }

      public void AddItem(string item)
      {
         SendOrPostCallback addItem = delegate(object text)
                                      {
                                         Items.Add(text);
                                      };
         try
         {
            m_SynchronizationContext.Send(addItem,item);
         }
         catch 
         {}
      }
   }
}
