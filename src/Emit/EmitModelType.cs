using System;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel.DataAnnotations;
using Huiali.EmitOData.Models;

namespace Huiali.EmitOData.Emit
{
    public static class EmitModelType
    {
        public static Type CreateModelType(this ModuleBuilder moduleBuilder, string connectionKey, Table table)
        {
            string typeName = $"{moduleBuilder.Assembly.GetName().Name}.{connectionKey}.Models.{table.Name}";
            TypeAttributes modelTypeAttr = TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, modelTypeAttr);
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
                        new object[] { });
                    propBldr.SetCustomAttribute(requiredAttribute);
                }
            }

            Type modeltype = typeBuilder.CreateType();
            return modeltype;
        }
    }
}