namespace CrmScriptLister.Tests
{
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void ParsesToCsv()
        {
            var xml = File.ReadAllText("customizations.xml");
            var parser = new Parser();

            var res = parser.ToCSV(xml);

            Assert.IsNotEmpty(res);
        }
    }
}
