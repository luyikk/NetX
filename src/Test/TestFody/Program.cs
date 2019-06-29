using Fody;
using System;

namespace TestFody
{
    class Program
    {
        static void Main(string[] args)
        {
            string assemblyPath =Environment.CurrentDirectory+ "\\ConsoleApp2.dll";
            new ModuleWeaver().ExecuteTestRun(assemblyPath, false);
        }
    }
}
