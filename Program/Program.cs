using System;
using System.Linq;
using System.Text;

namespace Program
{
    using System.IO;
    using CrmScriptLister;

    class Program
    {
        static void Main(string[] args)
        {
            var inputFilePath = args[0];

            var xml = File.ReadAllText(inputFilePath);

            Console.WriteLine("Reading File " + inputFilePath);
            Console.WriteLine("File size is " + xml.Count());

            var lister = new Parser();

            var res = lister.ToCSV(xml);

            try
            {
                using (var fs = new FileStream("js.csv", FileMode.OpenOrCreate))
                {
                    using (var writer = new StreamWriter(fs, Encoding.UTF8))
                    {
                        writer.Write(res);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Can not write the result. js.csv is open in other program.");

                //throw;
            }

        }
    }
}
