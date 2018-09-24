using System;
using System.Reflection;
using System.Reflection.Emit;
using Huiali.ILOData.Models;

namespace Huiali.ILOData.ILEmit
{
    public static class ClrTypeBuilder
    {
        internal static Type CreateModelType(this ModuleBuilder modelBuilder, string namespaceName, Table table)
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
    }
}