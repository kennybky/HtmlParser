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

        private string GetHtmlTableString(string url)
        {
            HttpResponseMessage response = Client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            string html = response.Content.ReadAsStringAsync().Result;
            int tableStart = html.IndexOf("<table", StringComparison.OrdinalIgnoreCase);
            int tableEnd = html.IndexOf("</table>", StringComparison.OrdinalIgnoreCase) + 8;
            if (tableStart > 0 && tableEnd > tableStart)
                html = html.Substring(tableStart, (tableEnd - tableStart));
            return html;
        }
        private XmlNodeList GetRows(string htmlTable)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(htmlTable);
            return doc.GetElementsByTagName("tr");
        }

      
        /// <summary>
        /// Loads a list of elements from a html table in the url. Uses the table headers as Property Names
        /// </summary>
        /// <param name="url">The endpoint to fetch the html string</param>
        /// <param name="offsetIndex">The starting index for loading data Default is 0</param>
        /// <returns>Collection of Elements of type <typeparamref name="T"/></returns>
        public ICollection<T> GetTableInfo(string url, int offsetIndex = 0)
        {
            string html = GetHtmlTableString(url);
            XmlNodeList rows = GetRows(html);
            var head = rows.Item(0);
            IList<string> ColMap = CreateColMap(head);
            return LoadListFromXml(rows, ColMap, 1+ offsetIndex)?.ToList();
        }

        /// <summary>
        /// Loads a list of elements from a html table in the url
        /// </summary>
        /// <param name="url">The endpoint to fetch the html string</param>
        /// <param name="ColMap">A list of property names to load the data. It must be in order of the columns</param>
        /// <param name="offsetIndex">The starting index for loading data. Default is 0</param>
        /// <returns>Collection of Elements of type <typeparamref name="T"/></returns>
        public ICollection<T> GetTableInfo(string url, IList<string> ColMap, int offsetIndex = 0)
        {
            string html = GetHtmlTableString(url);
            XmlNodeList rows = GetRows(html);
            return LoadListFromXml(rows, ColMap, 0 + offsetIndex)?.ToList();
        }

        /// <summary>
        /// Loads a list of elements from Xml Data.
        /// </summary>
        /// <param name="rows">The Xml Node List to load data from</param>
        /// <param name="ColMap">A list of property names to load the data. It must be in the same order of xml elements</param>
        /// <param name="startIndex">The starting index for loading data. Default is 0</param>
        /// <returns>Collection of Elements of type <typeparamref name="T"/></returns>
        public IEnumerable<T> LoadListFromXml(XmlNodeList rows, IList<string> ColMap, int startIndex = 0)
        {
            int colCount = ColMap.Count();
            for (int i = startIndex; i < rows.Count; i++)
            {
                T item = new T();
                var node = rows.Item(i);
                var cols = node.ChildNodes;
                if (cols.Count < colCount)
                    continue;
                for (int j = 0; j < colCount; j++)
                {
                    string tableHeader = ColMap[j].ToUpper();
                    PropertyInfo prop = Properties.FirstOrDefault(x => x.Name.ToUpper() == tableHeader);
                    if (prop == null)
                        continue;
                    XmlNode column = cols.Item(j);
                    string value = column.InnerText;
                    object safeValue = GetSafeValue(prop.PropertyType, value);
                    prop.SetValue(item, safeValue);
                }
                yield return item;

            }
        }


        public object GetSafeValue(Type t, string value)
        {
            Type underlyingType = Nullable.GetUnderlyingType(t);
            if (string.IsNullOrWhiteSpace(value))
            {
                //null value
                if(underlyingType != null)
                {
                    return null; //nullable, return null
                }
                else
                {
                    //not nullable return default
                    return t.GetDefaultValue();
                }
            }
            else
            {
                // not null, do cast
                Type convertType = underlyingType ?? t;
                return Convert.ChangeType(value, convertType);
            }
        }

        private IList<string> CreateColMap(XmlNode head)
        {
            XmlNodeList headCols = head.ChildNodes;
            IList<string> ColMap = new List<string>();
            for (int k = 0; k < headCols.Count; k++)
                ColMap.Insert(k, headCols[k].InnerText.Trim().ToUpper());
            return ColMap;
        }
    }
}
