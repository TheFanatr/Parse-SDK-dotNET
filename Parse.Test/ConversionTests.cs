using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parse.Utilities;

namespace Parse.Test
{
    [TestClass]
    public class ConversionTests
    {
        struct DummyValueTypeA { }

        struct DummyValueTypeB { }

        [TestMethod]
        public void TestToWithConstructedNullablePrimitive() => Assert.IsTrue(ConversionHelpers.DowncastValue<int?>((double) 4) is int?);

        [TestMethod]
        public void TestToWithConstructedNullableNonPrimitive() => Assert.ThrowsException<InvalidCastException>(() => ConversionHelpers.DowncastValue<DummyValueTypeA?>(new DummyValueTypeB { }));
    }
}
