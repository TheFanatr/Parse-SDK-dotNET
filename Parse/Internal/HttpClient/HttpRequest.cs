using System;
using System.Collections.Generic;
using System.IO;

namespace Parse.Common.Internal
{
    public class HttpRequest
    {
        public Uri Uri { get; set; }

        public IList<KeyValuePair<string, string>> Headers { get; set; }

        public virtual Stream Data { get; set; }

        public string Method { get; set; }
    }
}
