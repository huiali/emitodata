using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Huiali.EmitOData.Emit
{
    public static class EmitDbContextType
    {
        public static Type CreateDbContextType(this ModuleBuilder moduleBuilder, string connectionKey, List<Type> types)
        {
            string typeName = $"{moduleBuilder.Assembly.GetName().Name}.{connectionKey}.Models.{connectionKey}Context";
            var dbContexttype = typeof(DbContext);
            Type listOf = typeof(DbSet<>);
            TypeAttributes contextTypeAttr = TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, contextTypeAttr, dbContexttype);
            foreach (var type in types)
            {
                var modelTypeName = type.Name;
                Type listOfTFirst = listOf.MakeGenericType(type);
                FieldBuilder fieldBldr = typeBuilder.DefineField("_" + modelTypeName.ToLower(),
                    listOfTFirst,
                    FieldAttributes.Private);
                PropertyBuilder propBldr = typeBuilder.DefineProperty(modelTypeName,
                    PropertyAttributes.HasDefault,
                    listOfTFirst,
                    null);
                const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName |
                                                   MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;
                MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + modelTypeName,
                    getSetAttr,
                    listOfTFirst,
                    Type.EmptyTypes);
                ILGenerator getIlGenerator = getPropMthdBldr.GetILGenerator();
                getIlGenerator.Emit(OpCodes.Ldarg_0);
                getIlGenerator.Emit(OpCodes.Ldfld, fieldBldr);
                getIlGenerator.Emit(OpCodes.Ret);
                MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + modelTypeName,
                    getSetAttr,
                    null,
                    new[] { listOfTFirst });
                ILGenerator setIlGenerator = setPropMthdBldr.GetILGenerator();
                setIlGenerator.Emit(OpCodes.Ldarg_0);
                setIlGenerator.Emit(OpCodes.Ldarg_1);
                setIlGenerator.Emit(OpCodes.Stfld, fieldBldr);
                setIlGenerator.Emit(OpCodes.Ret);
                propBldr.SetGetMethod(getPropMthdBldr);
                propBldr.SetSetMethod(setPropMthdBldr);
            }
            Type toptionType = typeof(DbContextOptions<>);
            Type optionType = toptionType.MakeGenericType(typeBuilder);
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            ConstructorBuilder ctor1 = typeBuilder.DefineConstructor(
                methodAttributes,
                CallingConventions.Standard, new[] { optionType });
            ILGenerator ctor0Il = ctor1.GetILGenerator();
            ConstructorInfo method = dbContexttype.GetConstructor(new[] { typeof(DbContextOptions) });
            ctor0Il.Emit(OpCodes.Ldarg_0);
            ctor0Il.Emit(OpCodes.Ldarg_1);
            ctor0Il.Emit(OpCodes.Call, method);
            ctor0Il.Emit(OpCodes.Nop);
            ctor0Il.Emit(OpCodes.Nop);
            ctor0Il.Emit(OpCodes.Ret);

            return typeBuilder.CreateType();
        }
    }
}