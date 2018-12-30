[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("Parse")]

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [CompilerGlobalScope]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        public string AssemblyName { get; }

        public IgnoresAccessChecksToAttribute(string assemblyName) => AssemblyName = assemblyName;
    }
}
