using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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

        [TestMethod]
        public void TestMethod2()
        {
            var parser = new HtmlTableParser<OsType>();
            IList<string> ColMap = new string[] { "ID", "Name" };
            var list = parser.GetTableInfo("http://localhost:8000/table.html", ColMap, 1);
            Assert.IsNotNull(list);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var parser = new HtmlTableParser<OsType2>();
            var list = parser.GetTableInfo("http://localhost:8000/table.html");
            Assert.IsNotNull(list);
        }

        [TestMethod]
        public void TestMethod4()
        {
            var parser = new HtmlTableParser<OsType2>();
            var list = parser.GetTableInfo("http://localhost:8000/table2.html");
            Assert.IsNotNull(list);
        }
    }


    public class OsType
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class OsType2
    {
        public int? Id { get; set; }

        public string Name { get; set; }
    }
}
