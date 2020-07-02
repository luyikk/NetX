using Fody;
using System;

namespace TestFody
{
    class Program
    {
        static void Main(string[] args)
        {
            string assemblyPath = Environment.CurrentDirectory + "\\NetxActor.dll";
            new ModuleWeaver().ExecuteTestRun(assemblyPath, false);
        }
    }
}
