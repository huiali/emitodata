using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Huiali.EmitOData.Models;
using System.Linq.Expressions;

namespace Huiali.EmitOData.Emit
{
    public static class EmitDbContextType
    {
        public static Type CreateDbContextType(this ModuleBuilder moduleBuilder, string connectionKey, List<Entry> entrys)
        {
            //Define Type
            string typeName = $"{moduleBuilder.Assembly.GetName().Name}.{connectionKey}.Models.{connectionKey}Context";
            var dbContexttype = typeof(DbContext);
            Type listOf = typeof(DbSet<>);
            TypeAttributes contextTypeAttr = TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, contextTypeAttr, dbContexttype);

            //Define Constructor
            Type toptionType = typeof(DbContextOptions<>);
            Type optionType = toptionType.MakeGenericType(typeBuilder);
            MethodAttributes ctormethodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            ConstructorBuilder ctor1 = typeBuilder.DefineConstructor(
                ctormethodAttributes,
                CallingConventions.Standard, new[] { optionType });
            ILGenerator ctor0Il = ctor1.GetILGenerator();
            ConstructorInfo ctormethod = dbContexttype.GetConstructor(new[] { typeof(DbContextOptions) });
            ctor0Il.Emit(OpCodes.Ldarg_0);
            ctor0Il.Emit(OpCodes.Ldarg_1);
            ctor0Il.Emit(OpCodes.Call, ctormethod);
            ctor0Il.Emit(OpCodes.Nop);
            ctor0Il.Emit(OpCodes.Nop);
            ctor0Il.Emit(OpCodes.Ret);

            //Define Property
            foreach (var entry in entrys)
            {
                var modelTypeName = entry.Type.Name;
                Type listOfTFirst = listOf.MakeGenericType(entry.Type);
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

            //Define OnModelCreating
            MethodAttributes omcMthdAttrs =
                            MethodAttributes.Family
                            | MethodAttributes.Virtual
                            | MethodAttributes.HideBySig;
            MethodBuilder omcMthdBldr = typeBuilder.DefineMethod("OnModelCreating",
                     omcMthdAttrs,
                     typeof(void),
                     new[] { typeof(ModelBuilder) });

            ILGenerator omcMthdIlGen = omcMthdBldr.GetILGenerator();
            MethodInfo entityMthd = typeof(ModelBuilder).GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(p => p.GetParameters().Count() == 0 && p.Name == "Entity" && p.IsGenericMethod && p.IsVirtual);
            MethodInfo ToTableMthd = typeof(RelationalEntityTypeBuilderExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(p => p.Name == "ToTable" && p.GetParameters().Count() == 3 && p.IsGenericMethod);
            MethodInfo hasKeyMethod = typeof(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder).GetMethod("HasKey");

            omcMthdIlGen.Emit(OpCodes.Nop);
            foreach (var entry in entrys)
            {
                string[] keys = entry.Table.Columns.Where(p => p.IsPrimaryKey).Select(p => p.ColumnName).ToArray();
                int keysCount = keys.Count();
                OpCode countOpCode = (OpCode)typeof(OpCodes).GetField($"Ldc_I4_{keysCount}").GetValue(null);

                omcMthdIlGen.Emit(OpCodes.Ldarg_1);
                omcMthdIlGen.Emit(OpCodes.Callvirt, entityMthd.MakeGenericMethod(entry.Type));
                omcMthdIlGen.Emit(OpCodes.Ldstr, entry.Table.Name);
                omcMthdIlGen.Emit(OpCodes.Ldstr, entry.Table.Schema);
                omcMthdIlGen.Emit(OpCodes.Call, ToTableMthd.MakeGenericMethod(entry.Type));

                omcMthdIlGen.Emit(countOpCode);
                omcMthdIlGen.Emit(OpCodes.Newarr, typeof(string));
                for (int i = 0; i < keysCount; i++)
                {
                    OpCode itemOpCode = (OpCode)typeof(OpCodes).GetField($"Ldc_I4_{i}").GetValue(null);
                    omcMthdIlGen.Emit(OpCodes.Dup);
                    omcMthdIlGen.Emit(itemOpCode);
                    omcMthdIlGen.Emit(OpCodes.Ldstr, keys[i]);
                    omcMthdIlGen.Emit(OpCodes.Stelem_Ref);
                }
                omcMthdIlGen.Emit(OpCodes.Callvirt, hasKeyMethod);
                omcMthdIlGen.Emit(OpCodes.Pop);
            }

            omcMthdIlGen.Emit(OpCodes.Ret);
            var dbContextOmcMthd = typeof(DbContext).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(p => p.Name == "OnModelCreating" && p.IsVirtual && p.GetParameters().Count() == 1);
            typeBuilder.DefineMethodOverride(omcMthdBldr, dbContextOmcMthd);
            return typeBuilder.CreateType();
        }
    }
}
