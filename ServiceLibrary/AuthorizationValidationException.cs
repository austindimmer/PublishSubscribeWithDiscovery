using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary
{
    [Serializable]
    public class AuthorizationValidationException : ApplicationException
    {
        public AuthorizationValidationException(string message)
            : this(message, null)
        {
        }

        public AuthorizationValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
