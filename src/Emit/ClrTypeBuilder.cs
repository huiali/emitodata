using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Huiali.EmitOData.Emit
{
    public static class ClrTypeBuilder
    {
        internal static ModuleBuilder GetModuleBuilder()
        {
            string name = "Huiali.EmitOData.DynamicAssembly";
            AssemblyName assemblyName = new AssemblyName(name);
            assemblyName.Version = new Version(1, 0, 0, 0);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(name);
            return moduleBuilder;
        }
    }
}