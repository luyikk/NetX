using Fody;
using System;

namespace TestFody
{
    class Program
    {
        static void Main(string[] args)
        {
            string assemblyPath =Environment.CurrentDirectory+ "\\Server.dll";
            new ModuleWeaver().ExecuteTestRun(assemblyPath, false);
        }
    }
}
