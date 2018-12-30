using System.Collections.Generic;
using System.Linq;
using Parse.Core.Internal;

namespace Parse.Push.Internal
{
    public class ParseInstallationCoder : IParseInstallationCoder
    {
        public static ParseInstallationCoder Instance { get; } = new ParseInstallationCoder();

        const string ISO8601Format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

        public IDictionary<string, object> Encode(ParseInstallation installation)
        {
            IObjectState state = installation.GetState();
            IDictionary<string, object> data = PointerOrLocalIdEncoder.Instance.Encode(state.ToDictionary(x => x.Key, x => x.Value)) as IDictionary<string, object>;
            data["objectId"] = state.ObjectId;

            if (state.CreatedAt != null)
                data["createdAt"] = state.CreatedAt.Value.ToString(ISO8601Format);
            if (state.UpdatedAt != null)
                data["updatedAt"] = state.UpdatedAt.Value.ToString(ISO8601Format);

            return data;
        }

        public ParseInstallation Decode(IDictionary<string, object> data) => ParseObjectExtensions.FromState<ParseInstallation>(ParseObjectCoder.Instance.Decode(data, ParseDecoder.Instance), "_Installation");
    }
}