using System;

namespace Parse.Core.Internal
{
    public static class ParseFileExtensions
    {
        public static ParseFile Create(string name, Uri uri, string mimeType = null) => new ParseFile(name, uri, mimeType);
    }
}
