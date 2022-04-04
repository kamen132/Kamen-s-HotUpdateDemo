using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix
{
    public delegate void TestDelegateMethod(int a);
    public delegate string TestDelegateFunction(int a);
    class TestClass
    {
        private string str;
        public string Str
        {
            get
            {
                return str;
            }
        }
        public TestClass()
        {

        }

        public TestClass(string str)
        {
            this.str = str;
        }

        public static void staticFunTest()
        {
            Debug.Log("Test HotFix 调用热更代码成功！！！！");
        }

        public static void staticFunTest2(string str)
        {
            Debug.Log("Test HotFix 调用热更代码成功！！！！2  "+ str);
        }

        public static void GenericMethod<T>(T a)
        {
            Debug.LogError("TestClass GenericMethod  a=" + a);
        }
    }
}
