using System;
using System.Text.RegularExpressions;

namespace Parse
{
    [ParseClassName("_Role")]
    public class ParseRole : ParseObject
    {
        static Regex NamePattern { get; } = new Regex("^[0-9a-zA-Z_\\- ]+$");

        public ParseRole() : base() { }

        public ParseRole(string name, ParseACL control) : this()
        {
            Name = name;
            ACL = control;
        }

        [ParseFieldName("name")]
        public string Name
        {
            get => GetProperty<string>("Name");
            set => SetProperty(value, "Name");
        }

        [ParseFieldName("users")]
        public ParseRelation<ParseUser> Users => GetRelationProperty<ParseUser>("Users");

        [ParseFieldName("roles")]
        public ParseRelation<ParseRole> Roles => GetRelationProperty<ParseRole>("Roles");

        internal override void OnSettingValue(ref string key, ref object value)
        {
            base.OnSettingValue(ref key, ref value);
            if (key == "name")
            {
                if (ObjectId is null)
                    throw new InvalidOperationException("A role's name can only be set before it has been saved.");
                if (!(value is string))
                    throw new ArgumentException("A role's name must be a string.", nameof(value));
                if (!NamePattern.IsMatch((string) value))
                    throw new ArgumentException("A role's name can only contain alphanumeric characters, _, -, and spaces.", nameof(value));
            }
        }

        public static ParseQuery<ParseRole> Query => new ParseQuery<ParseRole>();
    }
}
