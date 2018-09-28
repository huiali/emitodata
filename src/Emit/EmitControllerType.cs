using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace Huiali.EmitOData.Emit
{
    public static class EmitControllerType
    {
        public static Type CreateControllerType(this ModuleBuilder moduleBuilder, string connectionKey, Type modeltype, Type contextType)
        {
            string controllerTypeName = $"{moduleBuilder.Assembly.GetName().Name}.{connectionKey}.Controllers.{modeltype.Name}Controller";
            TypeAttributes controllerTypeAttr = TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit;
            TypeBuilder typeBuilder = moduleBuilder.DefineType(controllerTypeName, controllerTypeAttr, typeof(ODataController));
            ConstructorInfo classCtorInfo = typeof(ProducesAttribute).GetConstructor(new Type[] { typeof(string), typeof(string[]) });
            CustomAttributeBuilder producesAttribute = new CustomAttributeBuilder(
                classCtorInfo,
                new object[] { "application/json", new string[0] });
            typeBuilder.SetCustomAttribute(producesAttribute);
            FieldBuilder fieldBldr = typeBuilder.DefineField("_context", contextType, FieldAttributes.Private);
            Type[] parameterTypes = { contextType };

            MethodAttributes controllerCtorAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            ConstructorBuilder controllerCtor = typeBuilder.DefineConstructor(
                controllerCtorAttr,
                CallingConventions.Standard, parameterTypes);
            ILGenerator ctor0Il = controllerCtor.GetILGenerator();
            var constructor = typeof(ODataController).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();

            ctor0Il.Emit(OpCodes.Ldarg_0);
            ctor0Il.Emit(OpCodes.Call, constructor);
            ctor0Il.Emit(OpCodes.Nop);
            ctor0Il.Emit(OpCodes.Ldarg_0);
            ctor0Il.Emit(OpCodes.Ldarg_1);
            ctor0Il.Emit(OpCodes.Stfld, fieldBldr);
            ctor0Il.Emit(OpCodes.Ret);

            const MethodAttributes getSetAttr = MethodAttributes.Public |
                                   MethodAttributes.HideBySig;

            Type returnType = typeof(IQueryable<>).MakeGenericType(typeBuilder);
            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("Get", getSetAttr, returnType, Type.EmptyTypes);

            ConstructorInfo enableQueryCtorInfo = typeof(EnableQueryAttribute).GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder enableQueryAttribute = new CustomAttributeBuilder(
                enableQueryCtorInfo,
                new object[] { });

            getPropMthdBldr.SetCustomAttribute(enableQueryAttribute);
            ILGenerator iLGenerator = getPropMthdBldr.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, fieldBldr);
            iLGenerator.Emit(OpCodes.Callvirt, contextType.GetMethod($"get_{modeltype.Name}"));
            iLGenerator.Emit(OpCodes.Ret);

            return typeBuilder.CreateType();
        }
    }
}