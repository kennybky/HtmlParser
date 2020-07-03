using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlParser
{
    internal static class TypeExtensions
    {
        public static object GetDefaultValue(this Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }
    }
}
