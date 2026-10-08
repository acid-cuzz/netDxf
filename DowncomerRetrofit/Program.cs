using System;
using System.IO;
using System.Linq;
using netDxf;

namespace DowncomerRetrofit
{
    /// <summary>
    /// Writes the DWG-DCINS-01..03 drawing set (downcomer retrofit insert, concept rev B) as DXF files.
    /// Usage: DowncomerRetrofit [output folder]
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            string folder = args.Length > 0 ? args[0] : "Drawings";
            Directory.CreateDirectory(folder);

            (string Name, Func<Sheet> Build)[] sheets =
            {
                ("DWG-DCINS-01.dxf", Sheet1.Build),
                ("DWG-DCINS-02.dxf", Sheet2.Build),
                ("DWG-DCINS-03.dxf", Sheet3.Build)
            };

            foreach ((string name, Func<Sheet> build) in sheets)
            {
                string file = Path.Combine(folder, name);
                Sheet sheet = build();
                if (!sheet.Doc.Save(file))
                {
                    Console.Error.WriteLine("Could not write " + file);
                    return 1;
                }

                // read it back as a check that the file is well formed
                DxfDocument check = DxfDocument.Load(file);
                if (check == null || check.Entities.All.Count() != sheet.Doc.Entities.All.Count())
                {
                    Console.Error.WriteLine("Read-back check failed for " + file);
                    return 1;
                }
                Console.WriteLine("{0} ({1} entities)", file, check.Entities.All.Count());
            }
            return 0;
        }
    }
}
