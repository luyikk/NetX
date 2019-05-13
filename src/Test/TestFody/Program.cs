using Fody;
using System;

namespace TestFody
{
    class Program
    {
        static void Main(string[] args)
        {
            string assemblyPath =Environment.CurrentDirectory+ "\\ActorTest.dll";
            new ModuleWeaver().ExecuteTestRun(assemblyPath, false);
        }
    }
}
