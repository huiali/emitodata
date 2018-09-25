using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using Huiali.ILOData.Models;
using Microsoft.EntityFrameworkCore;

namespace Huiali.ILOData.ILEmit
{
    public static class ClrTypeBuilder
    {
        public static Type CreateModelType(this ModuleBuilder modelBuilder, string namespaceName, Table table)
        {
            TypeBuilder typeBuilder = modelBuilder.DefineType(namespaceName + "." + table.Name, TypeAttributes.Public);
            // int i = 1;
            foreach (Column columnItem in table.Columns)
            {
                Type filedtype = columnItem.ColumnType;
                FieldBuilder fieldBldr = typeBuilder.DefineField("_" + columnItem.ColumnName.ToLower(),
                    filedtype,
                    FieldAttributes.Private);
                PropertyBuilder propBldr = typeBuilder.DefineProperty(columnItem.ColumnName,
                    PropertyAttributes.HasDefault,
                    filedtype,
                    null);
                const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName |
                                                    MethodAttributes.HideBySig;
                MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + columnItem.ColumnName,
                    getSetAttr,
                    filedtype,
                    Type.EmptyTypes);
                ILGenerator getIlGenerator = getPropMthdBldr.GetILGenerator();
                getIlGenerator.Emit(OpCodes.Ldarg_0);
                getIlGenerator.Emit(OpCodes.Ldfld, fieldBldr);
                getIlGenerator.Emit(OpCodes.Ret);
                MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + columnItem.ColumnName,
                    getSetAttr,
                    null,
                    new[] { filedtype });
                ILGenerator setIlGenerator = setPropMthdBldr.GetILGenerator();
                setIlGenerator.Emit(OpCodes.Ldarg_0);
                setIlGenerator.Emit(OpCodes.Ldarg_1);
                setIlGenerator.Emit(OpCodes.Stfld, fieldBldr);
                setIlGenerator.Emit(OpCodes.Ret);
                propBldr.SetGetMethod(getPropMthdBldr);
                propBldr.SetSetMethod(setPropMthdBldr);

                if (columnItem.IsPrimaryKey)
                {
                    ConstructorInfo classCtorInfo = typeof(KeyAttribute).GetConstructor(new Type[0]);
                    CustomAttributeBuilder keyAttribute = new CustomAttributeBuilder(
                        classCtorInfo,
                        new object[0]);
                    propBldr.SetCustomAttribute(keyAttribute);
                }
                if (columnItem.MaxLength != -1)
                {
                    ConstructorInfo classCtorInfo = typeof(MaxLengthAttribute).GetConstructor(new Type[] { typeof(int) });
                    CustomAttributeBuilder maxLengthAttribute = new CustomAttributeBuilder(
                        classCtorInfo,
                        new object[] { columnItem.MaxLength });
                    propBldr.SetCustomAttribute(maxLengthAttribute);
                }
                if (!columnItem.IsNullable)
                {
                    ConstructorInfo classCtorInfo = typeof(RequiredAttribute).GetConstructor(new Type[] { });
                    CustomAttributeBuilder requiredAttribute = new CustomAttributeBuilder(
                        classCtorInfo,
                        new object[] {  });
                    propBldr.SetCustomAttribute(requiredAttribute);
                }
            }
            Type modeltype = typeBuilder.CreateType();
            return modeltype;
        }

        public static Type CreateDbContext(this ModuleBuilder modelBuilder, string typeName, List<Type> types)
        {
            var dbContexttype = typeof(DbContext);
            Type listOf = typeof(DbSet<>);
            TypeBuilder typeBuilder = modelBuilder.DefineType(typeName, TypeAttributes.Public, dbContexttype);
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
            ConstructorBuilder ctor1 = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
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