using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix
{
    class TestDelegate
    {

        static TestDelegateMethod delegateMethod;
        static TestDelegateFunction delegateFunc;

        public static void Initialize()
        {
            delegateMethod = Method;
            delegateFunc = TestFunction;
        }
        
        public static void RunTest()
        {
            delegateMethod?.Invoke(44);
            delegateFunc?.Invoke(55);
        }

        static void Method(int a)
        {
            Debug.LogError("Test Method a==" + a);
        }

        static string TestFunction(int a)
        {
            Debug.LogError("Test Function  a===" + a);
            return a.ToString();
        }
    }




}
