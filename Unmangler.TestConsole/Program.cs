using System;
using System.Linq;

namespace Unmangler.TestConsole
{
    internal static class Program
    {
        private static void Main()
        {
            const string mangledStr = "?h@@YAXH@Z";
            var unmangler = new MSVCUnmangler(mangledStr);
            var unmangledStr = unmangler.Unmangle();

            if (unmangler.Failures.Any())
                Console.WriteLine($"Failed to unmangle: {unmangler.Failures.First()}");

            Console.WriteLine($"Mangled: {mangledStr} Unmangled: {unmangledStr}");
        }
    }
}
