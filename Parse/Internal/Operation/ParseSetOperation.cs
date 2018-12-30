namespace Parse.Core.Internal
{
    public class ParseSetOperation : IParseFieldOperation
    {
        public ParseSetOperation(object value) => Value = value;

        public object Encode() => PointerOrLocalIdEncoder.Instance.Encode(Value);

        public IParseFieldOperation MergeWithPrevious(IParseFieldOperation previous) => this;

        public object Apply(object oldValue, string key) => Value;

        public object Value { get; private set; }
    }
}
