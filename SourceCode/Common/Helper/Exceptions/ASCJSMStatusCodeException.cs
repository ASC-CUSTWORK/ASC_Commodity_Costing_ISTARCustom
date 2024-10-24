using ASCJSMCustom.Common.Descriptor;
using PX.Common;
using System;
using System.Net;
using System.Runtime.Serialization;

namespace ASCJSMCustom.Common.Helper.Exceptions
{
    /// <summary>
    /// Exception class for handling HTTP status code errors in ASCIStar API requests.
    /// </summary>
    /// <remarks>
    /// This exception is thrown when an ASCIStar API request returns an HTTP status code other than OK (200).
    /// It contains the status code and content of the response for error handling.
    /// </remarks>
    public class ASCJSMStatusCodeException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code of the failed request.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets the content of the failed request response.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ASCJSMStatusCodeException"/> class with the specified status code and response content.
        /// </summary>
        /// <param name="statusCode">The HTTP status code returned by the failed request.</param>
        /// <param name="content">The content of the failed request response.</param>
        public ASCJSMStatusCodeException(HttpStatusCode statusCode, string content) : base(string.Format(ASCJSMMessages.StatusCode.StatusCodeError, statusCode.ToString(), content))
        {
            StatusCode = statusCode;
            Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the ASCIStarStatusCodeException class with the serialized data and context information.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected ASCJSMStatusCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Overrides the default implementation of the GetObjectData method to add custom data to the SerializationInfo object. 
        /// This method is used during serialization to save the object's state to a stream and is required for types that implement the ISerializable interface.
        /// </summary>
        /// <param name="info">The SerializationInfo object to populate with data.</param>
        /// <param name="context">The destination (see StreamingContext) for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }
    }
}
