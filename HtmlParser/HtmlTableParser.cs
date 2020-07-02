using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Xml;

namespace HtmlParser
{
    public class HtmlTableParser<T> where T: class, new()
    {
        private readonly HttpClient Client = new HttpClient();


        public HtmlTableParser()
        {
            Properties = typeof(T).GetProperties();
        }

        public PropertyInfo[] Properties { get; }

        public ICollection<T> GetTableInfo(string url, bool headers = true)
        {
            var response = Client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            string html = response.Content.ReadAsStringAsync().Result;
            int tableStart = html.IndexOf("<table", StringComparison.OrdinalIgnoreCase);
            int tableEnd = html.IndexOf("</table>", StringComparison.OrdinalIgnoreCase) + 8;
            if(tableStart > 0 && tableEnd > tableStart)
                html = html.Substring(tableStart, (tableEnd - tableStart));
            return ParseHtml(html, headers).ToList();
        }

        public IEnumerable<T> ParseHtml(string html, bool headers = true)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(html);
            XmlNodeList rows = doc.GetElementsByTagName("tr");
            var head = rows.Item(0);
            var headCols = head.ChildNodes;
            for (int i = 1; i < rows.Count; i++)
            {
                T item = new T();
                var node  = rows.Item(i);
                var cols = node.ChildNodes;
                for(int j = 0; j < headCols.Count; j++)
                {
                    XmlNode header = headCols.Item(j);
                    string tableHeader = header.InnerText.Trim().ToUpper();
                    PropertyInfo prop = Properties.First(x => x.Name.ToUpper() == tableHeader);
                    XmlNode column = cols.Item(j);
                    object value = Convert.ChangeType(column.InnerText, prop.PropertyType);
                    prop.SetValue(item, value);
                }
                yield return item;
            }
        }
    }
}
