using Fody;
using System;

namespace TestFody
{
    class Program
    {
        static void Main(string[] args)
        {
            string assemblyPath = "TestClient.dll";
            new ModuleWeaver().ExecuteTestRun(assemblyPath, false);
        }
    }
}
