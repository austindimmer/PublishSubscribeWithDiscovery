using System;


// Excellent Bindings Table http://eaverae.nl/wcftable.html

// MSDN Streaming Message Transfer
// https://msdn.microsoft.com/en-us/library/ms731913.aspx
//.NET Framework 4.5

//Windows Communication Foundation(WCF) transports support two modes for transferring messages:

//    Buffered transfers hold the entire message in a memory buffer until the transfer is complete.A buffered message must be completely delivered before a receiver can read it.

//   Streamed transfers expose the message as a stream.The receiver starts processing the message before it is completely delivered.


//   Streamed transfers can improve the scalability of a service by eliminating the requirement for large memory buffers.Whether changing the transfer mode improves scalability depends on the size of the messages being transferred. Large message sizes favor using streamed transfers.

//By default, the HTTP, TCP/IP, and named pipe transports use buffered transfers.This document describes how to switch these transports from a buffered to streamed transfer mode and the consequences of doing so.
//Enabling Streamed Transfers

//Selecting between buffered and streamed transfer modes is done on the binding element of the transport.The binding element has a TransferMode property that can be set to Buffered, Streamed, StreamedRequest, or StreamedResponse. Setting the transfer mode to Streamed enables streaming communication in both directions. Setting the transfer mode to StreamedRequest or StreamedResponse enables streaming communication in the indicated direction only.


//The BasicHttpBinding, NetTcpBinding, and NetNamedPipeBinding bindings expose the TransferMode property.For other transports, you must create a custom binding to set the transfer mode.

//The decision to use either buffered or streamed transfers is a local decision of the endpoint. For HTTP transports, the transfer mode does not propagate across a connection, or to servers and other intermediaries. Setting the transfer mode is not reflected in the description of the service interface. After generating a client class for a service, you must edit the configuration file for services intended to be used with streamed transfers to set the mode.For TCP and named pipe transports, the transfer mode is propagated as a policy assertion.

//For code samples, see How to: Enable Streaming.
//Enabling Asynchronous Streaming

//To enable asynchronous streaming, add the DispatcherSynchronizationBehavior endpoint behavior to the service host and set its AsynchronousSendEnabled property to true.

//This version of WCF also adde the capability of true asynchronous streaming on the send side. This improves scalability of the service in scenarios where it is streaming messages to multiple clients some of which are slow in reading; possibly due to network congestion or are not reading at all.In these scenarios WCF no longer blocks individual threads on the service per client.This ensures that the service is able to process many more clients thereby improving the scalability of the service.
//Restrictions on Streamed Transfers


//Using the streamed transfer mode causes the run time to enforce additional restrictions.

//Operations that occur across a streamed transport can have a contract with at most one input or output parameter.That parameter corresponds to the entire body of the message and must be a Message, a derived type of Stream, or an IXmlSerializable implementation. Having a return value for an operation is equivalent to having an output parameter.


//Some WCF features, such as reliable messaging, transactions, and SOAP message-level security, rely on buffering messages for transmissions.Using these features may reduce or eliminate the performance benefits gained by using streaming. To secure a streamed transport, use transport-level security only or use transport-level security plus authentication-only message security.

//SOAP headers are always buffered, even when the transfer mode is set to streamed.The headers for a message must not exceed the size of the MaxBufferSize transport quota.For more information about this setting, see Transport Quotas.
//Differences Between Buffered and Streamed Transfers


//Changing the transfer mode from buffered to streamed also changes the native channel shape of the TCP and named pipe transports.For buffered transfers, the native channel shape is IDuplexSessionChannel.For streamed transfers, the native channels are IRequestChannel and IReplyChannel.Changing the transfer mode in an existing application that uses these transports directly (that is, not through a service contract) requires changing the expected channel shape for channel factories and listeners.

namespace ServiceLibrary
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public static class CustomBindings
    {

        public static Binding CreateRemoteExecutionSecureWindowsTcpBinding(string bindingNamespace)
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;


            //Be careful here too large a value on timeouts or received size can make it easy for DDOS type attacks.



            //            You'll want something like this to increase the message size quotas:

            //            < bindings >
            //                < basicHttpBinding >
            //                    < binding name = "basicHttp" allowCookies = "true"
            //                 maxReceivedMessageSize = "20000000"
            //                 maxBufferSize = "20000000"
            //                 maxBufferPoolSize = "20000000" >
            //            < readerQuotas maxDepth = "32"
            //                 maxArrayLength = "200000000"
            //                 maxStringContentLength = "200000000" />
            //        </ binding >
            //    </ basicHttpBinding >
            //</ bindings >

            //The justification for the values is simple, they are sufficiently large to accommodate most messages.You can tune that number to fit your needs.The low default value is basically there to prevent DOS type attacks.Making it 20000000 would allow for a distributed DOS attack to be effective, the default size of 64k would require a very large number of clients to overpower most servers these days.


            //There was a communication problem.The maximum message size quota for incoming messages (65536) has been exceeded.To increase the quota, use the MaxReceivedMessageSize property on the appropriate binding element.
            //binding.MaxReceivedMessageSize = Int32.MaxValue;  // = 2147483647


            //http://stackoverflow.com/questions/7370692/problem-of-setting-maxreceivedmessagesize-and-maxbuffersize-in-wcf-application
            // System.ArgumentException For TransferMode.Buffered, MaxReceivedMessageSize and MaxBufferSize must be the same value.\r\nParameter name: bindingElement"}	System.ArgumentException
            // Int32.MaxValue = 2147483647
            //transport.MaxReceivedMessageSize = Int32.MaxValue;
            //transport.MaxReceivedMessageSize = 2147483647;
            //transport.MaxBufferSize = Int32.MaxValue;


            //It's because of the TransferMode. You should set it to streamed, if you want to use a different buffer size than message size.
            //transports support two modes of transferring messages in each direction:
            //Buffered transfers hold the entire message in a memory buffer until the transfer is complete.
            //Streamed transfers only buffer the message headers and expose the message body as a stream, from which smaller portions can be read at a time.


            //An ProtocolException was received. The.Net Framing mode being used is not supported by 'net.tcp://austinm14x:15676/VoiceShortCutsRemoteExecutionService'.

            //One of the ways this error occurs is if there is a mismatch in the configuration of the client and the server.
            //The default is buffered, if one is set to streaming, when they try to talk to each other you get a framing error.
            //It thinks that each batch of data that the buffered tries to send over is a frame.

            // MSDN Streaming Message Transfer
            // https://msdn.microsoft.com/en-us/library/ms731913.aspx


            //binding.TransferMode = TransferMode.Buffered;
            binding.TransferMode = TransferMode.Streamed;

            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MaxBufferSize = int.MaxValue;
            binding.ReaderQuotas.MaxArrayLength = int.MaxValue;
            binding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
            binding.ReaderQuotas.MaxDepth = int.MaxValue;
            binding.OpenTimeout = TimeSpan.FromSeconds(15);
            binding.CloseTimeout = TimeSpan.FromSeconds(15);
            binding.ReceiveTimeout = TimeSpan.FromSeconds(600);
            binding.SendTimeout = TimeSpan.FromSeconds(600);
            
            if (bindingNamespace != null)
            {
                binding.Namespace = bindingNamespace;
            }

            if (bindingNamespace == null || bindingNamespace == String.Empty)
            {
                binding.Namespace = "https://www.effective-computing.com/";
            }

            binding.PortSharingEnabled = false;

            return binding;
        }
       
    }
}
