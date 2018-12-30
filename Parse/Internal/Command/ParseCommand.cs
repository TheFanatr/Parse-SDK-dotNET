using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Parse.Common.Internal;
using System.Linq;

namespace Parse.Core.Internal
{
    public class ParseCommand : HttpRequest
    {
        public IDictionary<string, object> DataObject { get; private set; }

        public override Stream Data
        {
            get => base.Data ?? (base.Data = DataObject is null ? null : new MemoryStream(Encoding.UTF8.GetBytes(JsonProcessor.Encode(DataObject))));
            set => base.Data = value;
        }

        public ParseCommand(string relativeUri, string method, string sessionToken = null, IList<KeyValuePair<string, string>> headers = null, IDictionary<string, object> data = null) : this(relativeUri, method, sessionToken, headers: headers, null, data is null ? null : "application/json") => DataObject = data;

        public ParseCommand(string relativeUri, string method, string sessionToken = null, IList<KeyValuePair<string, string>> headers = null, Stream stream = null, string contentType = null)
        {
            Uri = new Uri(new Uri(ParseClient.CurrentConfiguration.ServerURI), relativeUri);
            Method = method;
            Data = stream;
            Headers = new List<KeyValuePair<string, string>>(headers ?? Enumerable.Empty<KeyValuePair<string, string>>());

            if (!String.IsNullOrEmpty(sessionToken))
                Headers.Add(new KeyValuePair<string, string>("X-Parse-Session-Token", sessionToken));
            if (!String.IsNullOrEmpty(contentType))
                Headers.Add(new KeyValuePair<string, string>("Content-Type", contentType));
        }

        public ParseCommand(ParseCommand other)
        {
            Uri = other.Uri;
            Method = other.Method;
            DataObject = other.DataObject;
            Headers = new List<KeyValuePair<string, string>>(other.Headers);
            Data = other.Data;
        }
    }
}
