using System;
using System.Collections.Generic;
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
            foreach (Column columnItme in table.Columns)
            {
                Type filedtype = columnItme.ColumnType;
                FieldBuilder fieldBldr = typeBuilder.DefineField("_" + columnItme.ColumnName.ToLower(),
                    filedtype,
                    FieldAttributes.Private);
                PropertyBuilder propBldr = typeBuilder.DefineProperty(columnItme.ColumnName,
                    PropertyAttributes.HasDefault,
                    filedtype,
                    null);
                const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName |
                                                    MethodAttributes.HideBySig;
                MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + columnItme.ColumnName,
                    getSetAttr,
                    filedtype,
                    Type.EmptyTypes);
                ILGenerator getIlGenerator = getPropMthdBldr.GetILGenerator();
                getIlGenerator.Emit(OpCodes.Ldarg_0);
                getIlGenerator.Emit(OpCodes.Ldfld, fieldBldr);
                getIlGenerator.Emit(OpCodes.Ret);
                MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + columnItme.ColumnName,
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
            }
            Type modeltype = typeBuilder.CreateType();
            return modeltype;
        }

        public static Type CreateDbContext(this ModuleBuilder modelBuilder, string typeName, List<Type> types, string connectionString)
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

            Type[] parameterTypes = { };
            ConstructorBuilder ctor1 = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard, parameterTypes);

            ILGenerator ctor0Il = ctor1.GetILGenerator();

            ConstructorInfo method = dbContexttype.GetConstructor(new[] { typeof(string) });

            ctor0Il.Emit(OpCodes.Ldarg_0);
            ctor0Il.Emit(OpCodes.Ldstr, connectionString);
            ctor0Il.Emit(OpCodes.Call, method);
            ctor0Il.Emit(OpCodes.Nop);
            ctor0Il.Emit(OpCodes.Nop);
            ctor0Il.Emit(OpCodes.Nop);
            ctor0Il.Emit(OpCodes.Ret);

            return typeBuilder.CreateType();
        }
    }
}