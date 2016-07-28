using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary
{
    public class FaultHandledOperations
    {
        
        public static T ExecuteFaultHandledOperation<T>(Func<T> codetoExecute)
        {
            //USAGE

            //return FaultHandledOperations.ExecuteFaultHandledOperation(() =>
            //{
            //    //Code here
            //    return typeToReturn;
            //});

            try
            {
                return codetoExecute.Invoke();
            }
            // For WCF Exceptions docs see http://msdn.microsoft.com/en-us/library/ms789039.aspx
            // http://delicious.com/austindimmer/WCF+exceptions
            catch (ArgumentException ex)
            {
                Debug.WriteLine("The service host has a problem ArgumentException. " + ex.Message);
                throw new FaultException<ArgumentException>(ex, ex.Message);
            }
            catch (AuthorizationValidationException ex)
            {
                throw new FaultException<AuthorizationValidationException>(ex, ex.Message);
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine("The service operation timed out. " + ex.Message);
                throw new FaultException<TimeoutException>(ex, ex.Message);
            }
            catch (AddressAlreadyInUseException ex)
            {
                Debug.WriteLine("An AddressAlreadyInUseException was received. " + ex.Message);
                throw new FaultException<AddressAlreadyInUseException>(ex, ex.Message);
            }
            catch (AddressAccessDeniedException ex)
            {
                Debug.WriteLine("An AddressAccessDeniedException was received. " + ex.Message);
                throw new FaultException<AddressAccessDeniedException>(ex, ex.Message);
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Debug.WriteLine("A CommunicationObjectFaultedException was received. " + ex.Message);
                throw new FaultException<CommunicationObjectFaultedException>(ex, ex.Message);
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Debug.WriteLine("A CommunicationObjectAbortedException was received. " + ex.Message);
                throw new FaultException<CommunicationObjectAbortedException>(ex, ex.Message);
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine("An EndpointNotFoundException was received. " + ex.Message);
                throw new FaultException<EndpointNotFoundException>(ex, ex.Message);
            }
            catch (ProtocolException ex)
            {
                Debug.WriteLine("A ProtocolException was received. " + ex.Message);
                throw new FaultException<ProtocolException>(ex, ex.Message);
            }
            catch (ServerTooBusyException ex)
            {
                Debug.WriteLine("A ServerTooBusyException was received. " + ex.Message);
                throw new FaultException<ServerTooBusyException>(ex, ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine("An ObjectDisposedException was received. " + ex.Message);
                throw new FaultException<ObjectDisposedException>(ex, ex.Message);
            }            catch (FaultException ex)
            {
                Debug.WriteLine("An FaultException was received. " + ex.Message);
                throw;
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine("There was a communication problem. " + ex.Message + ex.StackTrace);
                throw new FaultException<CommunicationException>(ex, ex.Message);
            }
            catch (Exception){
                throw;
            }
        }

        public static void ExecuteFaultHandledOperation(Action codetoExecute)
        {
            //USAGE

            //FaultHandledOperations.ExecuteFaultHandledOperation(() =>
            //{
            //    //Code here
                
            //});

            try
            {
                codetoExecute.Invoke();
            }
            // For WCF Exceptions docs see http://msdn.microsoft.com/en-us/library/ms789039.aspx
            // http://delicious.com/austindimmer/WCF+exceptions
            catch (AuthorizationValidationException ex)
            {
                throw new FaultException<AuthorizationValidationException>(ex, ex.Message);
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine("The service operation timed out. " + ex.Message);
                throw new FaultException<TimeoutException>(ex, ex.Message);
            }
            catch (AddressAlreadyInUseException ex)
            {
                Debug.WriteLine("An AddressAlreadyInUseException was received. " + ex.Message);
                throw new FaultException<AddressAlreadyInUseException>(ex, ex.Message);
            }
            catch (AddressAccessDeniedException ex)
            {
                Debug.WriteLine("An AddressAccessDeniedException was received. " + ex.Message);
                throw new FaultException<AddressAccessDeniedException>(ex, ex.Message);
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Debug.WriteLine("A CommunicationObjectFaultedException was received. " + ex.Message);
                throw new FaultException<CommunicationObjectFaultedException>(ex, ex.Message);
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Debug.WriteLine("A CommunicationObjectAbortedException was received. " + ex.Message);
                throw new FaultException<CommunicationObjectAbortedException>(ex, ex.Message);
            }
            catch (EndpointNotFoundException ex)
            {
                Debug.WriteLine("An EndpointNotFoundException was received. " + ex.Message);
                throw new FaultException<EndpointNotFoundException>(ex, ex.Message);
            }
            catch (ProtocolException ex)
            {
                Debug.WriteLine("A ProtocolException was received. " + ex.Message);
                throw new FaultException<ProtocolException>(ex, ex.Message);
            }
            catch (ServerTooBusyException ex)
            {
                Debug.WriteLine("A ServerTooBusyException was received. " + ex.Message);
                throw new FaultException<ServerTooBusyException>(ex, ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine("An ObjectDisposedException was received. " + ex.Message);
                throw new FaultException<ObjectDisposedException>(ex, ex.Message);
            }
            catch (FaultException ex)
            {
                Debug.WriteLine("An FaultException was received. " + ex.Message);
                throw;
            }
            catch (CommunicationException ex)
            {
                Debug.WriteLine("There was a communication problem. " + ex.Message + ex.StackTrace);
                throw new FaultException<CommunicationException>(ex, ex.Message);
            }
            catch (Exception){
                throw;
            }
        }
    }
}
