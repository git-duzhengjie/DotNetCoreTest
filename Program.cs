using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dynamic.Core;
using RabbitMQ.Client.Events;

namespace ConsoleApp1
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //IPoint point = ParseInterface<IPoint>();
            //Test test = new Test();
            //test.SendMessage();
            IPerson p = CreateType<IPerson>();
            //p.SayHello("hello", "word");
            //p.SayHello3("hello", "word", "!");
            Console.WriteLine(p.GetMessage("hello").Value);
            //p.Test();
            //RabbitMqHelper rabbitMqHelper = new RabbitMqHelper("amqp://192.168.137.2:5672/", "guest", "guest", 2);
            //string hello = Console.ReadLine();
            //Debug.Assert(hello != null, nameof(hello) + " != null");
            ////rabbitMqHelper.SendMessage("test", "test", "", Encoding.UTF8.GetBytes(hello));
            //int count = 0;

            //void Process(object ob, BasicDeliverEventArgs ea)
            //{
            //    Console.WriteLine($"{count++}:{Encoding.UTF8.GetString(ea.Body)}");
            //}

            //rabbitMqHelper.ReadMessage("test", Process);
            Console.ReadLine();
        }


        public static T CreateType<T>()
        {
            //AssemblyName assemblyName = new AssemblyName("assemblyName");
            //AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            //ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("PersonModule", "Person.dll");
            //TypeBuilder typeBuilder = moduleBuilder.DefineType("Person", TypeAttributes.Public);
            //*添加所实现的接口
            var type = typeof(T);
            TypeBuilder typeBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("sdfs"), AssemblyBuilderAccess.Run)
                .DefineDynamicModule(type.GetTypeInfo().Module.Name)
                .DefineType(type.FullName ?? throw new InvalidOperationException(), TypeAttributes.NotPublic);
            typeBuilder.AddInterfaceImplementation(typeof(IPerson));
            //Type[] conTypes = {typeof(string)};
            //const MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName 
            //                                                                     | MethodAttributes.RTSpecialName | MethodAttributes.Public;
            //var constructor = typeBuilder.DefineConstructor(methodAttributes, CallingConventions.HasThis, conTypes);
            //var cil = constructor.GetILGenerator();
            //cil.Emit(OpCodes.Ldarg_0);
            //for (int i = 1; i < conTypes.Length; i++)
            //{
            //    cil.Emit(OpCodes.Ldarg, i);
            //}
            //cil.Emit(OpCodes.Call, typeof(object).GetConstructor(conTypes) ?? throw new InvalidOperationException());
            //cil.Emit(OpCodes.Ret);
            MethodInfo[] methods = type.GetMethods();
            //实现方法
            foreach (var m in methods)
            {
                ParameterInfo[] parameter = m.GetParameters();
                Type[] array = parameter.Select(p => p.ParameterType).ToArray();
                MethodBuilder mbIm = typeBuilder.DefineMethod(m.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig |
                    MethodAttributes.NewSlot | MethodAttributes.Virtual |
                    MethodAttributes.Final,
                    m.ReturnType,
                    array);

                ILGenerator il = mbIm.GetILGenerator();
                
                LocalBuilder local = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Ldc_I4, parameter.Length);
                il.Emit(OpCodes.Newarr, typeof(object));
                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldstr, type.FullName);
                il.Emit(OpCodes.Ldtoken, m.ReturnType);
                il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }));
                for (int i = 0; i < parameter.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc, local);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, i+1);
                    Type t = array[i];
                    if (type.GetTypeInfo().IsValueType || type.IsGenericParameter)
                        il.Emit(OpCodes.Box, t);
                    il.Emit(OpCodes.Stelem_Ref);
                }
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Call, typeof(Test).GetMethod("GetMessage",
                                          new Type[] { typeof(string), typeof(Type), typeof(object[])}) ?? throw new InvalidOperationException());
                il.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(mbIm, typeof(IPerson).GetMethod(m.Name) ?? throw new InvalidOperationException());
            }
            Type personType = typeBuilder.CreateType();
            return (T)Activator.CreateInstance(personType);
        }


        //static T ParseInterface<T>() where T:class,IPoint
        //{
        //    var type = typeof(T);
        //    TypeBuilder typeBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("sdfs"), AssemblyBuilderAccess.Run)
        //        .DefineDynamicModule(type.GetTypeInfo().Module.Name)
        //        .DefineType(type.FullName ?? throw new InvalidOperationException(), TypeAttributes.NotPublic);
        //    typeBuilder.AddInterfaceImplementation(type);

        //    //FieldBuilder fieldBuilder =
        //    //    typeBuilder.DefineField("interceptor", type, FieldAttributes.Private | FieldAttributes.InitOnly);
        //    //FieldBuilder fieldApiMethods = typeBuilder.DefineField(nameof(methods), typeof(MethodInfo[]),
        //    //FieldAttributes.Private | FieldAttributes.InitOnly);
        //    Type objType = Type.GetType("System.Object");
        //    ConstructorInfo objCtor = objType.GetConstructor(new Type[0]);
        //    Type[] types = new Type[] {typeof(int)};
        //    FieldBuilder xField = typeBuilder.DefineField("x", typeof(int),
        //        FieldAttributes.Public);
        //    ILGenerator ilConstructGenerator = typeBuilder
        //        .DefineConstructor(MethodAttributes.Public,
        //            CallingConventions.Standard, types).GetILGenerator();
        //    ilConstructGenerator.Emit(OpCodes.Ldarg_0);
        //    ilConstructGenerator.Emit(OpCodes.Call, objCtor);
        //    ilConstructGenerator.Emit(OpCodes.Ldarg_0);
        //    ilConstructGenerator.Emit(OpCodes.Ldarg_1);
        //    ilConstructGenerator.Emit(OpCodes.Stfld, xField);
        //    ilConstructGenerator.Emit(OpCodes.Ret);
        //    MethodInfo[] methods = type.GetMethods();
        //    foreach (var m in methods)
        //    {
        //        var mth = typeBuilder.DefineMethod(m.Name, MethodAttributes.Public,
        //            typeof(int),
        //            null);
        //        var mIl = mth.GetILGenerator();
        //        switch (m.Name)
        //        {
        //            case "GetX":
        //                mIl.Emit(OpCodes.Ldfld, xField);
        //                break;
        //        }
        //        mIl.Emit(OpCodes.Ret);
        //        //mIl.Emit(OpCodes.Nop);
        //                //mIl.Emit(OpCodes.Ldarg_0);
        //                //mIl.Emit(OpCodes.Ldfld);
        //                //mIl.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }) ?? throw new InvalidOperationException());
        //                //mIl.Emit(OpCodes.Nop);
        //                //mIl.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadLine") ?? throw new InvalidOperationException());
        //                //mIl.Emit(OpCodes.Pop);
        //                //mIl.Emit(OpCodes.Ret);
        //        }

        //    return (T)typeBuilder.CreateTypeInfo().GetConstructor(types)?.Invoke(new object[] { 4});
        //}
    }
}
