using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Parse.Common.Internal
{
    /// <summary>
    /// A simple recursive-descent JSON Parser based on the grammar defined at http://www.json.org
    /// and http://tools.ietf.org/html/rfc4627
    /// </summary>
    public class JsonProcessor
    {
        static readonly char[] falseValue = "false".ToCharArray(), trueValue = "true".ToCharArray(), nullValue = "null".ToCharArray();

        static readonly Regex numberValue = new Regex(@"\G-?(?:0|[1-9]\d*)(?<frac>\.\d+)?(?<exp>(?:e|E)(?:-|\+)?\d+)?"), stringValue = new Regex(@"\G""(?<content>(?:[^\\""]|(?<escape>\\(?:[\\""/bfnrt]|u[0-9a-fA-F]{{4}})))*)""", RegexOptions.Multiline), escapePattern = new Regex("\\\\|\"|[\u0000-\u001F]");

        public string Input { get; private set; }

        public char[] InputAsArray { get; private set; }

        public int CurrentIndex { get; private set; }

        public void Skip(int skip) => CurrentIndex += skip;

        public JsonProcessor(string input)
        {
            Input = input;
            InputAsArray = input.ToCharArray();
        }

        /// <summary>
        /// Parses JSON object syntax (e.g. '{}')
        /// </summary>
        bool ParseObject(out object output)
        {
            output = null;
            int initialCurrentIndex = CurrentIndex;
            if (!Find('{'))
                return false;

            Dictionary<string, object> dict = new Dictionary<string, object> { };
            while (true)
            {
                if (!ParseMember(out object pairValue))
                    break;

                Tuple<string, object> pair = pairValue as Tuple<string, object>;
                dict[pair.Item1] = pair.Item2;
                if (!Find(','))
                    break;
            }
            if (!Find('}'))
                return false;
            output = dict;
            return true;
        }

        /// <summary>
        /// Parses JSON member syntax (e.g. '"keyname" : null')
        /// </summary>
        bool ParseMember(out object output)
        {
            output = null;
            if (!ParseString(out object key))
                return false;
            if (!Find(':'))
                return false;
            if (!ParseValue(out object value))
                return false;
            output = new Tuple<string, object>((string) key, value);
            return true;
        }

        /// <summary>
        /// Parses JSON array syntax (e.g. '[]')
        /// </summary>
        bool ParseArray(out object output)
        {
            output = null;
            if (!Find('['))
                return false;

            List<object> list = new List<object> { };
            while (true)
            {
                if (!ParseValue(out object value))
                    break;
                list.Add(value);
                if (!Find(','))
                    break;
            }
            if (!Find(']'))
                return false;
            output = list;
            return true;
        }

        /// <summary>
        /// Parses a value (i.e. the right-hand side of an object member assignment or
        /// an element in an array)
        /// </summary>
        bool ParseValue(out object output)
        {
            if (Find(falseValue))
            {
                output = false;
                return true;
            }
            else if (Find(nullValue))
            {
                output = null;
                return true;
            }
            else if (Find(trueValue))
            {
                output = true;
                return true;
            }
            return ParseObject(out output) || ParseArray(out output) || ParseNumber(out output) || ParseString(out output);
        }

        /// <summary>
        /// Parses a JSON string (e.g. '"foo\u1234bar\n"')
        /// </summary>
        bool ParseString(out object output)
        {
            output = null;
            if (!Match(stringValue, out Match m))
                return false;

            int offset = 0;
            Group contentCapture = m.Groups["content"];
            StringBuilder builder = new StringBuilder(contentCapture.Value);
            foreach (Capture escape in m.Groups["escape"].Captures)
            {
                int index = escape.Index - contentCapture.Index - offset;
                offset += escape.Length - 1;
                builder.Remove(index + 1, escape.Length - 1);
                switch (escape.Value[1])
                {
                    case '\"':
                        builder[index] = '\"';
                        break;
                    case '\\':
                        builder[index] = '\\';
                        break;
                    case '/':
                        builder[index] = '/';
                        break;
                    case 'b':
                        builder[index] = '\b';
                        break;
                    case 'f':
                        builder[index] = '\f';
                        break;
                    case 'n':
                        builder[index] = '\n';
                        break;
                    case 'r':
                        builder[index] = '\r';
                        break;
                    case 't':
                        builder[index] = '\t';
                        break;
                    case 'u':
                        builder[index] = (char) UInt16.Parse(escape.Value.Substring(2), NumberStyles.AllowHexSpecifier);
                        break;
                    default:
                        throw new ArgumentException("Unexpected escape character in string: " + escape.Value);
                }
            }
            output = builder.ToString();
            return true;
        }

        /// <summary>
        /// Parses a number. Returns a long if the number is an integer or has an exponent,
        /// otherwise returns a double.
        /// </summary>
        bool ParseNumber(out object output)
        {
            output = null;
            if (!Match(numberValue, out Match m))
                return false;
            if (m.Groups["frac"].Length > 0 || m.Groups["exp"].Length > 0)
            {
                output = Double.Parse(m.Value, CultureInfo.InvariantCulture);
                return true;
            }
            else
            {
                output = Int64.Parse(m.Value, CultureInfo.InvariantCulture);
                return true;
            }
        }

        /// <summary>
        /// Matches the string to a regex, consuming part of the string and returning the match.
        /// </summary>
        bool Match(Regex matcher, out Match target)
        {
            target = matcher.Match(Input, CurrentIndex);
            if (target.Success)
                Skip(target.Length);
            return target.Success;
        }

        /// <summary>
        /// Find the first occurrences of a character, consuming part of the string.
        /// </summary>
        bool Find(char reference)
        {
            int step = 0, strLen = InputAsArray.Length, currentStep = CurrentIndex;
            char currentChar = default;

            RemoveWhitespace(ref currentStep, ref currentChar, ref step, strLen);

            bool match = currentStep < strLen && InputAsArray[currentStep] == reference;
            if (match)
            {
                step++;
                currentStep++;

                RemoveWhitespace(ref currentStep, ref currentChar, ref step, strLen);
                Skip(step);
            }
            return match;

        }

        /// <summary>
        /// Find the first occurrences of a string, consuming part of the string.
        /// </summary>
        bool Find(char[] reference)
        {
            int step = 0, strLen = InputAsArray.Length, currentStep = CurrentIndex;
            char currentChar = default;

            RemoveWhitespace(ref currentStep, ref currentChar, ref step, strLen);

            bool strMatch = true;
            for (int i = 0; currentStep < strLen && i < reference.Length; ++i, ++currentStep)
            {
                if (InputAsArray[currentStep] != reference[i])
                {
                    strMatch = false;
                    break;
                }
            }

            bool match = currentStep < strLen && strMatch;
            if (match)
                Skip(step + reference.Length);
            return match;
        }

        void RemoveWhitespace(ref int currentStep, ref char currentChar, ref int step, int strLen)
        {
            while (currentStep < strLen && ((currentChar = InputAsArray[currentStep]) == ' ' || currentChar == '\r' || currentChar == '\t' || currentChar == '\n'))
            {
                step++;
                currentStep++;
            }
        }

        /// <summary>
        /// Parses a JSON-text as defined in http://tools.ietf.org/html/rfc4627, returning an
        /// IDictionary&lt;string, object&gt; or an IList&lt;object&gt; depending on whether
        /// the value was an array or dictionary. Nested objects also match these types.
        /// </summary>
        public static object Parse(string input)
        {
            input = input.Trim();
            JsonProcessor parser = new JsonProcessor(input);

            if ((parser.ParseObject(out object output) || parser.ParseArray(out output)) && parser.CurrentIndex == input.Length)
                return output;
            throw new ArgumentException("Input JSON was invalid.");
        }

        /// <summary>
        /// Encodes a dictionary into a JSON string. Supports values that are
        /// IDictionary&lt;string, object&gt;, IList&lt;object&gt;, strings,
        /// nulls, and any of the primitive types.
        /// </summary>
        public static string Encode(IDictionary<string, object> dict)
        {
            if (dict == null)
                throw new ArgumentNullException();
            if (dict.Count == 0)
                return "{}";

            StringBuilder builder = new StringBuilder("{");
            foreach (KeyValuePair<string, object> pair in dict)
            {
                builder.Append(Encode(pair.Key));
                builder.Append(":");
                builder.Append(Encode(pair.Value));
                builder.Append(",");
            }
            builder[builder.Length - 1] = '}';
            return builder.ToString();
        }

        /// <summary>
        /// Encodes a list into a JSON string. Supports values that are
        /// IDictionary&lt;string, object&gt;, IList&lt;object&gt;, strings,
        /// nulls, and any of the primitive types.
        /// </summary>
        public static string Encode(IList<object> list)
        {
            if (list == null)
                throw new ArgumentNullException();
            if (list.Count == 0)
                return "[]";

            StringBuilder builder = new StringBuilder("[");
            foreach (object item in list)
            {
                builder.Append(Encode(item));
                builder.Append(",");
            }
            builder[builder.Length - 1] = ']';
            return builder.ToString();
        }

        /// <summary>
        /// Encodes an object into a JSON string.
        /// </summary>
        public static string Encode(object obj)
        {
            switch (obj)
            {
                case IDictionary<string, object> dict:
                    return Encode(dict);
                case IList<object> list:
                    return Encode(list);
                case string str:
                    str = escapePattern.Replace(str, m =>
                    {
                        switch (m.Value[0])
                        {
                            case '\\':
                                return @"\\";
                            case '\"':
                                return @"\""";
                            case '\b':
                                return @"\b";
                            case '\f':
                                return @"\f";
                            case '\n':
                                return @"\n";
                            case '\r':
                                return @"\r";
                            case '\t':
                                return @"\t";
                            default:
                                return $@"\u{((ushort) m.Value[0]).ToString("x4")}";
                        }
                    });
                    return $@"""{str}""";
                case null:
                    return "null";
                case bool value:
                    return value ? "true" : "false";
                case object value when !value.GetType().GetTypeInfo().IsPrimitive:
                    throw new ArgumentException($"Unable to encode objects of type {value.GetType()}");
                default:
                    return Convert.ToString(obj, CultureInfo.InvariantCulture);
            }
        }
    }
}
