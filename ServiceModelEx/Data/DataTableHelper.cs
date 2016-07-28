// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace ServiceModelEx
{
   public static class DataTableHelper
   {
      public static T[] ToArray<R,T>(this DataTable table,Func<R,T> converter) where R : DataRow
      {
         if(table.Rows.Count == 0)
         {
            return new T[]{};
         }
         //Verify [DataContract] or [Serializable] on T
         Debug.Assert(IsDataContract(typeof(T)) || typeof(T).IsSerializable);

         //Verify table contains correct rows 
         Debug.Assert(MatchingTableRow<R>(table));

         return table.Rows.Cast<R>().Select(converter).ToArray();
      }
      static bool IsDataContract(Type type)
      {
         object[] attributes = type.GetCustomAttributes(typeof(DataContractAttribute),false);
         return attributes.Length == 1;
      }
      static bool MatchingTableRow<R>(DataTable table)
      {
         if(table.Rows.Count == 0)
         {
            return true;
         }
         return table.Rows[0] is R;
      }
   }
}
