using Parse.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parse.Test
{
    [TestClass]
    public class JsonTests
    {
        [TestMethod]
        public void TestEmptyJsonStringFail() => Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse(""));

        [TestMethod]
        public void TestInvalidJsonStringAsRootFail()
        {
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("\n"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("a"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("abc"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("\u1234"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("\t"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("\t\n\r"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("   "));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("1234"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("1,3"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{1"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("3}"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("}"));
        }

        [TestMethod]
        public void TestEmptyJsonObject() => Assert.IsTrue(JsonProcessor.Parse("{}") is IDictionary);

        [TestMethod]
        public void TestEmptyJsonArray() => Assert.IsTrue(JsonProcessor.Parse("[]") is IList);

        [TestMethod]
        public void TestOneJsonObject()
        {
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{ 1 }"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{ 1 : 1 }"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{ 1 : \"abc\" }"));

            object parsed = JsonProcessor.Parse("{\"abc\" : \"def\"}");
            Assert.IsTrue(parsed is IDictionary);
            IDictionary parsedDict = parsed as IDictionary;
            Assert.AreEqual("def", parsedDict["abc"]);

            parsed = JsonProcessor.Parse("{\"abc\" : {} }");
            Assert.IsTrue(parsed is IDictionary);
            parsedDict = parsed as IDictionary;
            Assert.IsTrue(parsedDict["abc"] is IDictionary);

            parsed = JsonProcessor.Parse("{\"abc\" : \"6060\"}");
            Assert.IsTrue(parsed is IDictionary);
            parsedDict = parsed as IDictionary;
            Assert.AreEqual("6060", parsedDict["abc"]);

            parsed = JsonProcessor.Parse("{\"\" : \"\"}");
            Assert.IsTrue(parsed is IDictionary);
            parsedDict = parsed as IDictionary;
            Assert.AreEqual("", parsedDict[""]);

            parsed = JsonProcessor.Parse("{\" abc\" : \"def \"}");
            Assert.IsTrue(parsed is IDictionary);
            parsedDict = parsed as IDictionary;
            Assert.AreEqual("def ", parsedDict[" abc"]);

            parsed = JsonProcessor.Parse("{\"1\" : 6060}");
            Assert.IsTrue(parsed is IDictionary);
            parsedDict = parsed as IDictionary;
            Assert.AreEqual((long) 6060, parsedDict["1"]);

            parsed = JsonProcessor.Parse("{\"1\" : null}");
            Assert.IsTrue(parsed is IDictionary);
            parsedDict = parsed as IDictionary;
            Assert.IsNull(parsedDict["1"]);

            parsed = JsonProcessor.Parse("{\"1\" : true}");
            Assert.IsTrue(parsed is IDictionary);
            parsedDict = parsed as IDictionary;
            Assert.IsTrue((bool) parsedDict["1"]);

            parsed = JsonProcessor.Parse("{\"1\" : false}");
            Assert.IsTrue(parsed is IDictionary);
            parsedDict = parsed as IDictionary;
            Assert.IsFalse((bool) parsedDict["1"]);
        }

        [TestMethod]
        public void TestMultipleJsonObjectAsRootFail()
        {
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{},"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{\"abc\" : \"def\"},"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{\"abc\" : \"def\" \"def\"}"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{}, {}"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{},\n{}"));
        }

        [TestMethod]
        public void TestOneJsonArray()
        {
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("[ 1 : 1 ]"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("[ 1 1 ]"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("[ 1 : \"1\" ]"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("[ \"1\" : \"1\" ]"));

            object parsed = JsonProcessor.Parse("[ 1 ]");
            Assert.IsTrue(parsed is IList);
            IList parsedList = parsed as IList;
            Assert.AreEqual((long) 1, parsedList[0]);

            parsed = JsonProcessor.Parse("[ \n ]");
            Assert.IsTrue(parsed is IList);
            parsedList = parsed as IList;
            Assert.AreEqual(0, parsedList.Count);

            parsed = JsonProcessor.Parse("[ \"asdf\" ]");
            Assert.IsTrue(parsed is IList);
            parsedList = parsed as IList;
            Assert.AreEqual("asdf", parsedList[0]);

            parsed = JsonProcessor.Parse("[ \"\u849c\" ]");
            Assert.IsTrue(parsed is IList);
            parsedList = parsed as IList;
            Assert.AreEqual("\u849c", parsedList[0]);
        }

        [TestMethod]
        public void TestMultipleJsonArrayAsRootFail()
        {
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("[],"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("[\"abc\" : \"def\"],"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("[], []"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("[],\n[]"));
        }

        [TestMethod]
        public void TestJsonArrayInsideJsonObject()
        {
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{ [] }"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{ [], [] }"));
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("{ \"abc\": [], [] }"));

            object parsed = JsonProcessor.Parse("{ \"abc\": [] }");
            Assert.IsTrue(parsed is IDictionary);
            IDictionary parsedDict = parsed as IDictionary;
            Assert.IsTrue(parsedDict["abc"] is IList);

            parsed = JsonProcessor.Parse("{ \"6060\" :\n[ 6060 ]\t}");
            Assert.IsTrue(parsed is IDictionary);
            parsedDict = parsed as IDictionary;
            Assert.IsTrue(parsedDict["6060"] is IList);
            IList parsedList = parsedDict["6060"] as IList;
            Assert.AreEqual((long) 6060, parsedList[0]);
        }

        [TestMethod]
        public void TestJsonObjectInsideJsonArray()
        {
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("[ {} : {} ]"));

            // whitespace test
            object parsed = JsonProcessor.Parse("[\t\n{}\r\t]");
            Assert.IsTrue(parsed is IList);
            IList parsedList = parsed as IList;
            Assert.IsTrue(parsedList[0] is IDictionary);

            parsed = JsonProcessor.Parse("[ {}, { \"final\" : \"fantasy\"} ]");
            Assert.IsTrue(parsed is IList);
            parsedList = parsed as IList;
            Assert.IsTrue(parsedList[0] is IDictionary);
            Assert.IsTrue(parsedList[1] is IDictionary);
            IDictionary parsedDictionary = parsedList[1] as IDictionary;
            Assert.AreEqual("fantasy", parsedDictionary["final"]);
        }

        [TestMethod]
        public void TestJsonObjectWithElements()
        {
            // Just make sure they don't throw exception as we already check their content correctness
            // in other unit tests.
            JsonProcessor.Parse("{ \"mura\": \"masa\" }");
            JsonProcessor.Parse("{ \"mura\": 1234 }");
            JsonProcessor.Parse("{ \"mura\": { \"masa\": 1234 } }");
            JsonProcessor.Parse("{ \"mura\": { \"masa\": [ 1234 ] } }");
            JsonProcessor.Parse("{ \"mura\": { \"masa\": [ 1234 ] }, \"arr\": [] }");
        }

        [TestMethod]
        public void TestJsonArrayWithElements()
        {
            // Just make sure they don't throw exception as we already check their content correctness
            // in other unit tests.
            JsonProcessor.Parse("[ \"mura\" ]");
            JsonProcessor.Parse("[ \"\u1234\" ]");
            JsonProcessor.Parse("[ \"\u1234ff\", \"\u1234\" ]");
            JsonProcessor.Parse("[ [], [], [], [] ]");
            JsonProcessor.Parse("[ [], [ {}, {} ], [ {} ], [] ]");
        }

        [TestMethod]
        public void TestEncodeJson()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            string encoded = JsonProcessor.Encode(dict);
            Assert.AreEqual("{}", encoded);

            List<object> list = new List<object>();
            encoded = JsonProcessor.Encode(list);
            Assert.AreEqual("[]", encoded);

            Dictionary<string, object> dictChild = new Dictionary<string, object>();
            list.Add(dictChild);
            encoded = JsonProcessor.Encode(list);
            Assert.AreEqual("[{}]", encoded);

            list.Add("1234          a\t\r\n");
            list.Add(1234);
            list.Add(12.34);
            list.Add(1.23456789123456789);
            encoded = JsonProcessor.Encode(list);
            Assert.AreEqual("[{},\"1234          a\\t\\r\\n\",1234,12.34,1.23456789123457]", encoded);

            dict["arr"] = new List<object>();
            encoded = JsonProcessor.Encode(dict);
            Assert.AreEqual("{\"arr\":[]}", encoded);

            dict["\u1234"] = "\u1234";
            encoded = JsonProcessor.Encode(dict);
            Assert.AreEqual("{\"arr\":[],\"\u1234\":\"\u1234\"}", encoded);

            encoded = JsonProcessor.Encode(new List<object> { true, false, null });
            Assert.AreEqual("[true,false,null]", encoded);
        }

        [TestMethod]
        public void TestSpecialJsonNumbersAndModifiers()
        {
            Assert.ThrowsException<ArgumentException>(() => JsonProcessor.Parse("+123456789"));

            JsonProcessor.Parse("{ \"mura\": -123456789123456789 }");
            JsonProcessor.Parse("{ \"mura\": 1.1234567891234567E308 }");
            JsonProcessor.Parse("{ \"PI\": 3.141e-10 }");
            JsonProcessor.Parse("{ \"PI\": 3.141E-10 }");

            Assert.AreEqual(123456789123456789, (JsonProcessor.Parse("{ \"mura\": 123456789123456789 }") as IDictionary)["mura"]);
        }
    }
}
