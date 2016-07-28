// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

namespace ServiceModelEx.ServiceFabric.Services.Client
{
   public class ServicePartitionResolver
   {
      internal string FabricBaseAddress
      {get; set;}

      public ServicePartitionResolver() 
      {
         FabricBaseAddress = "localhost";
      }
      public ServicePartitionResolver(string fabricBaseAddress) 
      {
         FabricBaseAddress = fabricBaseAddress;
      }

      public static ServicePartitionResolver GetDefault()
      {
         return new ServicePartitionResolver();
      }
   }
}
