using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Cci;

namespace CciTest {
    class Program {
        static void Main(string[] args)
        {
            var fixturePath = @"C:\Users\mscottford\Documents\Visual Studio 2015\Projects\CciTest\Fixture\bin\Debug";
            var pdbFixturePath = Path.Combine(fixturePath, "Fixture.pdb");
            var peFixturePath = Path.Combine(fixturePath, "Fixture.dll");

            var host = new PeReader.DefaultHost();

            var pdbFileStream = File.OpenRead(pdbFixturePath);

            var peReader = new PeReader(host);
            var pdbReader = new PdbReader(pdbFileStream, host);

            var assembly = peReader.OpenAssembly(BinaryDocument.GetBinaryDocumentForFile(peFixturePath, host));

            foreach (var type in assembly.GetAllTypes())
            {
                foreach (var method in type.Methods)
                {
                    Console.WriteLine("Method Name:");
                    Console.WriteLine(method);

                    Console.WriteLine("Source Location:");
                    foreach (var sourceLocation in pdbReader.GetPrimarySourceLocationsFor(method.Locations))
                    {
                        Console.WriteLine($"{sourceLocation.SourceDocument.Name}:{sourceLocation.StartLine}");
                    }
                    
                    Console.WriteLine("---");
                }
            }

            Console.ReadLine();
        }
    }
}
