using System.Collections.Generic;

namespace Parse.Common.Internal
{
    public interface IJsonConvertible
    {
        IDictionary<string, object> ToJSON();
    }
}
