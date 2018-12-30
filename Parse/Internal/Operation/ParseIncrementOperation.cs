using System;
using System.Collections.Generic;
using System.Linq;
using Parse.Core.Internal;

namespace Parse.Core.Internal
{
    public class ParseIncrementOperation : IParseFieldOperation
    {
        // Defines adders for all of the implicit conversions: http://msdn.microsoft.com/en-US/library/y5b434w4(v=vs.80).aspx
        static IDictionary<(Type, Type), Func<object, object, object>> Adders { get; } = new Dictionary<(Type, Type), Func<object, object, object>>
        {
            [(typeof(sbyte), typeof(sbyte))] = (left, right) => (sbyte) left + (sbyte) right,
            [(typeof(sbyte), typeof(short))] = (left, right) => (sbyte) left + (short) right,
            [(typeof(sbyte), typeof(int))] = (left, right) => (sbyte) left + (int) right,
            [(typeof(sbyte), typeof(long))] = (left, right) => (sbyte) left + (long) right,
            [(typeof(sbyte), typeof(float))] = (left, right) => (sbyte) left + (float) right,
            [(typeof(sbyte), typeof(double))] = (left, right) => (sbyte) left + (double) right,
            [(typeof(sbyte), typeof(decimal))] = (left, right) => (sbyte) left + (decimal) right,
            [(typeof(byte), typeof(byte))] = (left, right) => (byte) left + (byte) right,
            [(typeof(byte), typeof(short))] = (left, right) => (byte) left + (short) right,
            [(typeof(byte), typeof(ushort))] = (left, right) => (byte) left + (ushort) right,
            [(typeof(byte), typeof(int))] = (left, right) => (byte) left + (int) right,
            [(typeof(byte), typeof(uint))] = (left, right) => (byte) left + (uint) right,
            [(typeof(byte), typeof(long))] = (left, right) => (byte) left + (long) right,
            [(typeof(byte), typeof(ulong))] = (left, right) => (byte) left + (ulong) right,
            [(typeof(byte), typeof(float))] = (left, right) => (byte) left + (float) right,
            [(typeof(byte), typeof(double))] = (left, right) => (byte) left + (double) right,
            [(typeof(byte), typeof(decimal))] = (left, right) => (byte) left + (decimal) right,
            [(typeof(short), typeof(short))] = (left, right) => (short) left + (short) right,
            [(typeof(short), typeof(int))] = (left, right) => (short) left + (int) right,
            [(typeof(short), typeof(long))] = (left, right) => (short) left + (long) right,
            [(typeof(short), typeof(float))] = (left, right) => (short) left + (float) right,
            [(typeof(short), typeof(double))] = (left, right) => (short) left + (double) right,
            [(typeof(short), typeof(decimal))] = (left, right) => (short) left + (decimal) right,
            [(typeof(ushort), typeof(ushort))] = (left, right) => (ushort) left + (ushort) right,
            [(typeof(ushort), typeof(int))] = (left, right) => (ushort) left + (int) right,
            [(typeof(ushort), typeof(uint))] = (left, right) => (ushort) left + (uint) right,
            [(typeof(ushort), typeof(long))] = (left, right) => (ushort) left + (long) right,
            [(typeof(ushort), typeof(ulong))] = (left, right) => (ushort) left + (ulong) right,
            [(typeof(ushort), typeof(float))] = (left, right) => (ushort) left + (float) right,
            [(typeof(ushort), typeof(double))] = (left, right) => (ushort) left + (double) right,
            [(typeof(ushort), typeof(decimal))] = (left, right) => (ushort) left + (decimal) right,
            [(typeof(int), typeof(int))] = (left, right) => (int) left + (int) right,
            [(typeof(int), typeof(long))] = (left, right) => (int) left + (long) right,
            [(typeof(int), typeof(float))] = (left, right) => (int) left + (float) right,
            [(typeof(int), typeof(double))] = (left, right) => (int) left + (double) right,
            [(typeof(int), typeof(decimal))] = (left, right) => (int) left + (decimal) right,
            [(typeof(uint), typeof(uint))] = (left, right) => (uint) left + (uint) right,
            [(typeof(uint), typeof(long))] = (left, right) => (uint) left + (long) right,
            [(typeof(uint), typeof(ulong))] = (left, right) => (uint) left + (ulong) right,
            [(typeof(uint), typeof(float))] = (left, right) => (uint) left + (float) right,
            [(typeof(uint), typeof(double))] = (left, right) => (uint) left + (double) right,
            [(typeof(uint), typeof(decimal))] = (left, right) => (uint) left + (decimal) right,
            [(typeof(long), typeof(long))] = (left, right) => (long) left + (long) right,
            [(typeof(long), typeof(float))] = (left, right) => (long) left + (float) right,
            [(typeof(long), typeof(double))] = (left, right) => (long) left + (double) right,
            [(typeof(long), typeof(decimal))] = (left, right) => (long) left + (decimal) right,
            [(typeof(char), typeof(char))] = (left, right) => (char) left + (char) right,
            [(typeof(char), typeof(ushort))] = (left, right) => (char) left + (ushort) right,
            [(typeof(char), typeof(int))] = (left, right) => (char) left + (int) right,
            [(typeof(char), typeof(uint))] = (left, right) => (char) left + (uint) right,
            [(typeof(char), typeof(long))] = (left, right) => (char) left + (long) right,
            [(typeof(char), typeof(ulong))] = (left, right) => (char) left + (ulong) right,
            [(typeof(char), typeof(float))] = (left, right) => (char) left + (float) right,
            [(typeof(char), typeof(double))] = (left, right) => (char) left + (double) right,
            [(typeof(char), typeof(decimal))] = (left, right) => (char) left + (decimal) right,
            [(typeof(float), typeof(float))] = (left, right) => (float) left + (float) right,
            [(typeof(float), typeof(double))] = (left, right) => (float) left + (double) right,
            [(typeof(ulong), typeof(ulong))] = (left, right) => (ulong) left + (ulong) right,
            [(typeof(ulong), typeof(float))] = (left, right) => (ulong) left + (float) right,
            [(typeof(ulong), typeof(double))] = (left, right) => (ulong) left + (double) right,
            [(typeof(ulong), typeof(decimal))] = (left, right) => (ulong) left + (decimal) right,
            [(typeof(double), typeof(double))] = (left, right) => (double) left + (double) right,
            [(typeof(decimal), typeof(decimal))] = (left, right) => (decimal) left + (decimal) right
        };

        static ParseIncrementOperation()
        {
            foreach ((Type, Type) pair in Adders.Keys.ToList())
            {
                if (pair.Item1.Equals(pair.Item2))
                    continue;

                Adders[(pair.Item2, pair.Item1)] = (left, right) => Adders[pair](right, left);
            }
        }

        public ParseIncrementOperation(object amount) => Amount = amount;

        public object Encode() => new Dictionary<string, object>
        {
            ["__op"] = "Increment",
            ["amount"] = Amount
        };

        private static object Add(object obj1, object obj2)
        {
            if (Adders.TryGetValue((obj1.GetType(), obj2.GetType()), out Func<object, object, object> adder))
                return adder(obj1, obj2);

            throw new InvalidCastException($"Cannot add {obj1.GetType()} to {obj2.GetType()}.");
        }

        public IParseFieldOperation MergeWithPrevious(IParseFieldOperation previous)
        {
            switch (previous)
            {
                case null:
                    return this;
                case ParseDeleteOperation _:
                    return new ParseSetOperation(Amount);
                case ParseSetOperation set:
                    if (set.Value is string)
                        throw new InvalidOperationException("Cannot increment a non-number type.");

                    return new ParseSetOperation(Add(set.Value, Amount));
                case ParseIncrementOperation increment:
                    return new ParseIncrementOperation(Add(increment.Amount, Amount));
                default:
                    throw new InvalidOperationException("Operation is invalid after previous operation.");
            }
        }

        public object Apply(object oldValue, string key)
        {
            if (oldValue is string)
                throw new InvalidOperationException("Cannot increment a non-number type.");

            return Add(oldValue ?? 0, Amount);
        }

        public object Amount { get; }
    }
}
