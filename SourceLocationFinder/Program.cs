using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Cci;

namespace SourceLocationFinder {
    class Program {
        static void Main(string[] args)
        {
            var searchPath = Directory.GetCurrentDirectory();

            if (args.Length > 0)
            {
                if (args[0] == "/?" || args[0] == "-h" || args[0] == "--help")
                {
                    Console.WriteLine("Usage: SourceLocationFinder.exe [path]");
                    Console.WriteLine("Walks a directory tree and attempts to find source locations for all methods");
                    Console.WriteLine("discovered in any .NET Assemblies which are accompanied by debugging info.");
                    Console.WriteLine("If path is not specified the current directory will be used.");
                    return;
                }

                searchPath = args[0];
            }

            var methodLocations = new Dictionary<string, string>();

            foreach (var pdbFile in Directory.EnumerateFiles(searchPath, "*.pdb", SearchOption.AllDirectories))
            {
                var dllFile = Path.ChangeExtension(pdbFile, "dll");
                var exeFile = Path.ChangeExtension(pdbFile, "exe");
                var peFile = (string) null;
                if (File.Exists(dllFile))
                {
                    peFile = dllFile;
                } 
                else if (File.Exists(exeFile))
                {
                    peFile = exeFile;
                }

                if (peFile != null)
                {
                    ReadMethodLocations(methodLocations, pdbFile, peFile);
                }
            }
            
            foreach (var keyPair in methodLocations)
            {
                Console.WriteLine($"{keyPair.Value}\t{keyPair.Key}");
            }
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
                foreach (var member in type.Members)
                {
                    foreach (var sourceLocation in pdbReader.GetPrimarySourceLocationsFor(member.Locations))
                    {
                        var memberName = $"{member}";

                        if (!methodLocations.ContainsKey(memberName))
                        {
                            methodLocations.Add(
                                memberName,
                                $"{sourceLocation.SourceDocument.Location}:{sourceLocation.StartLine}"
                            );
                        }
                    }
                }
            }
        }
    }
}
