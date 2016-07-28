// © 2016 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.ServiceBus;

namespace ServiceModelEx.ServiceBus
{
   public partial class ServiceBusGraph
   {
      string Token
      {get;set;}

      string Namespace
      {get;set;}      
      
      string Secret
      {get;set;}

      string Issuer
      {get;set;}

      public ServiceBusNode[] DiscoveredEndpoints
      {get;private set;}

      string m_ServiceBusRootAddress;

      public string ServiceBusRootAddress
      {
         get
         {
            return m_ServiceBusRootAddress;
         }
         set
         {
            m_ServiceBusRootAddress = value;
            if(m_ServiceBusRootAddress.StartsWith(@"/"))
            {
               m_ServiceBusRootAddress = m_ServiceBusRootAddress.Remove(0,1);
            }
            if(m_ServiceBusRootAddress.EndsWith(@"/"))
            {
               m_ServiceBusRootAddress = m_ServiceBusRootAddress.Remove(m_ServiceBusRootAddress.Length-1,1);
            }
         }
      }
      public readonly TransportClientEndpointBehavior Credential;

      public ServiceBusGraph(string serviceNamespace,string issuer,string secret)
      {
         Namespace = serviceNamespace;
         Secret = secret;
         Issuer = issuer;

         ServiceBusRootAddress = ServiceBusEnvironment.CreateServiceUri("https",serviceNamespace,"").AbsoluteUri;

         ServiceBusRootAddress = VerifyEndSlash(ServiceBusRootAddress);

         Credential = new TransportClientEndpointBehavior();
         Credential.TokenProvider = TokenProvider.CreateSharedSecretTokenProvider(Issuer,Secret);
      }


      public ServiceBusNode[] Discover()
      {
         DiscoveredEndpoints = null;

         if(Token == null)
         {
            Token = GetToken(Namespace,Secret);
         }

         List<ServiceBusNode> nodes = Discover(ServiceBusRootAddress,null);

         Consolidate(nodes);

         DiscoveredEndpoints = SortList(nodes);

         return DiscoveredEndpoints;
      }

      void Consolidate(List<ServiceBusNode> nodes)
      {
         //Routers and buffers subscriber, they will appear twice - once as routers or queues and once as policies
         //Keep just the policies
         List<ServiceBusNode> nodesToRemove = new List<ServiceBusNode>();

         foreach(ServiceBusNode part in nodes)
         {
            foreach(ServiceBusNode node in nodes)
            {
               if(node != part && node.Name.StartsWith(part.Name,StringComparison.OrdinalIgnoreCase))
               {
                  if(nodesToRemove.Contains(part) == false)
                  {
                     nodesToRemove.Add(part);
                  }
               }
            }
         }

         foreach(ServiceBusNode node in nodesToRemove)
         {
            nodes.Remove(node);
         }
      }


      ServiceBusNode[] SortList(List<ServiceBusNode> nodes)
      {
         ServiceBusNode[] array = new ServiceBusNode[nodes.Count];

         for(int i = 0;i<array.Length;i++)
         {
            ServiceBusNode maxNode = FindMax(nodes);
            array[i] = maxNode;
            nodes.Remove(maxNode);
         }
         //Transpose array
         ServiceBusNode[] returned = new ServiceBusNode[array.Length];

         int index = 0;
         for(int j = array.Length-1;j>=0;j--)
         {
            returned[index++] = array[j];
         }
         return returned;
      }
      ServiceBusNode FindMax(List<ServiceBusNode> nodes)
      {
         ServiceBusNode maxNode = new ServiceBusNode("");
         foreach(ServiceBusNode node in nodes)
         {
            if(StringComparer.Ordinal.Compare(node.Name,maxNode.Name) >= 0)
            {
               maxNode = node;
            }
         }
         return maxNode;
      }

      List<ServiceBusNode> Discover(string root,ServiceBusNode router)
      {
         root = VerifyNoEndSlash(root);

         Uri feedUri = new Uri(root);

         List<ServiceBusNode> nodes = new List<ServiceBusNode>();

         if(root.Contains("!") == false)
         {
            string relativeAddress = root.Replace(ServiceBusRootAddress,"");
            if(relativeAddress != "" && relativeAddress != "/")
            {
               ServiceBusNode node = new ServiceBusNode(root);
               nodes.Add(node);
            }
         }

         SyndicationFeed feed = GetFeed(feedUri,Token);

         if(feed != null)
         {
            foreach(SyndicationItem endpoint in feed.Items)
            {
               foreach(SyndicationLink link in endpoint.Links)
               {
                  Trace.WriteLine("Link: " + link.RelationshipType + " " + link.Uri.AbsoluteUri);
                  
                  nodes.AddRange(Discover(link.Uri.AbsoluteUri,router));
               }
            }
         }
         return nodes;
      }
                  

      string GetToken(string serviceNamespace,string password)
      {
         string token = null;

         //string tokenUri = string.Format("https://{0}/issuetoken.aspx?u={1}&p={2}",ServiceBusEnvironment.DefaultIdentityHostName,solutionName,Uri.EscapeDataString(solutionPassword));
         string tokenUri = Microsoft.ServiceBus.ServiceBusEnvironment.CreateServiceUri("https",serviceNamespace,"").AbsoluteUri;

         HttpWebRequest tokenRequest = WebRequest.Create(tokenUri) as HttpWebRequest;

         tokenRequest.Method = "GET";

         using(HttpWebResponse tokenResponse = tokenRequest.GetResponse() as HttpWebResponse)
         {
            StreamReader tokenStreamReader = new StreamReader(tokenResponse.GetResponseStream());

            token = tokenStreamReader.ReadToEnd();
         }
         return token;
      }
      static SyndicationFeed GetFeed(Uri feedUri,string token)
      {
         if(feedUri.Scheme != "http" && feedUri.Scheme != "https")
         {
            return null;
         }
         HttpWebRequest getFeedRequest = WebRequest.Create(feedUri) as HttpWebRequest;
         getFeedRequest.Method = "GET";
         getFeedRequest.Headers.Add("X-MS-Identity-Token",token);

         Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter();

         try
         {
            using(HttpWebResponse getFeedResponse = getFeedRequest.GetResponse() as HttpWebResponse)
            {
               atomFormatter.ReadFrom(new XmlTextReader(getFeedResponse.GetResponseStream()));
            }
         }
         catch
         {
         }
         return atomFormatter.Feed;
      }
      string VerifyEndSlash(string text)
      {
         Debug.Assert(text != null);

         if(text != String.Empty)
         {
            if(text.EndsWith("/") == false)
            {
               return text += "/";
            }
         }
         return text;
      }

      static string VerifyNoEndSlash(string text)
      {
         Debug.Assert(text != null);

         if(text != String.Empty)
         {
            if(text.EndsWith("/"))
            {
               return text.Remove(text.Length-1,1);
            }
         }
         return text;
      }      
   }
}