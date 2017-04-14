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
            var methodLocations = new Dictionary<string, string>();

            ReadMethodLocations(methodLocations, pdbFixturePath, peFixturePath);

            foreach (var keyPair in methodLocations)
            {
                Console.WriteLine("Method Name:");
                Console.WriteLine(keyPair.Key);

                Console.WriteLine("Source Location:");
                Console.WriteLine(keyPair.Value);

                Console.WriteLine("---");
            }

            Console.ReadLine();
        }

        private static void ReadMethodLocations(Dictionary<string, string> methodLocations, string pdbFixturePath, string peFixturePath)
        {
            var host = new PeReader.DefaultHost();

            var pdbFileStream = File.OpenRead(pdbFixturePath);

            var peReader = new PeReader(host);
            var pdbReader = new PdbReader(pdbFileStream, host);

            var assembly = peReader.OpenAssembly(
                BinaryDocument.GetBinaryDocumentForFile(peFixturePath, host)
            );

            foreach (var type in assembly.GetAllTypes())
            {
                foreach (var method in type.Members)
                {
                    foreach (var sourceLocation in pdbReader.GetPrimarySourceLocationsFor(method.Locations))
                    {
                        if (!methodLocations.ContainsKey(method.ToString()))
                        {
                            methodLocations.Add(
                                method.ToString(),
                                $"{sourceLocation.SourceDocument.Name}:{sourceLocation.StartLine}"
                            );
                        }
                    }
                }
            }
        }
    }
}
