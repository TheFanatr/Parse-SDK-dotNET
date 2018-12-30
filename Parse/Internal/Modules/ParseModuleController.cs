using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AssemblyLister;

namespace Parse.Common.Internal
{
    public class ParseModuleController
    {
        public static ParseModuleController Instance { get; } = new ParseModuleController();

        object Mutex { get; } = new object { };

        List<IParseModule> Modules { get; } = new List<IParseModule> { };

        bool Initialized { get; set; } = false;

        public void RegisterModule(IParseModule module)
        {
            if (module == null)
                return;

            lock (Mutex)
            {
                Modules.Add(module);
                module.OnModuleRegistered();

                if (Initialized)
                    module.OnParseInitialized();
            }
        }

        public void ScanForModules()
        {
            lock (Mutex)
            {
                foreach (Type moduleType in Lister.AllAssemblies.SelectMany(asm => asm.GetCustomAttributes<ParseModuleAttribute>()).Select(attr => attr.ModuleType).Where(type => type != null && type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IParseModule))))
                {
                    try
                    {
                        ConstructorInfo constructor = moduleType.FindConstructor();
                        if (constructor != null)
                            RegisterModule(constructor.Invoke(null) as IParseModule);
                    }
                    catch { }
                }
            }
        }

        public void Reset()
        {
            lock (Mutex)
            {
                Modules.Clear();
                Initialized = false;
            }
        }

        public void ParseDidInitialize()
        {
            lock (Mutex)
            {
                foreach (IParseModule module in Modules)
                    module.OnParseInitialized();
                Initialized = true;
            }
        }
    }
}