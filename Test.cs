using System;
using System.Linq;

namespace ConsoleApp1
{
    public static class Test
    {
        private static string init = "sdfsdfsdfsdf";
        public static void SayHello(object[] objects)
        {
            Console.WriteLine(string.Join("", objects)+init);
        }

        public static object GetMessage(string name, Type type, object[] objects)
        {
            return new Hello{Key = name,Value = type.FullName};
        }

        public static object GetMessage(Type name)
        {
            return name.FullName;
        }

        public static object TestP<T>()
        {
            return GetMessage(typeof(T));
        }
    }
}
