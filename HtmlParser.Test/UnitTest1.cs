using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HtmlParser.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var parser = new HtmlTableParser<OsType>();
            var list = parser.GetTableInfo("http://localhost:8000/table.html");
            Assert.IsNotNull(list);
        }
    }


    public class OsType
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
