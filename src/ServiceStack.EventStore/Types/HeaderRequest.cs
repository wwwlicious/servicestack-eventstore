namespace ServiceStack.EventStore.Types
{
    using System.Collections.Generic;
    using Host;

    public class HeaderRequest : BasicRequest
    {
        public HeaderRequest(IDictionary<string, string> headers)
        {
            Headers = headers;
        }

        public new IDictionary<string, string> Headers { get;  }
    }
}
