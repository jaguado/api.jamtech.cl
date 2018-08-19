using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Text;

namespace JAMTech.Extensions
{
    public static class Dynamic
    {
        public static ExpandoObject ToExpandoObject(this object obj)
        {
            // Null-check

            var expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(obj.GetType()))
            {
                expando.TryAdd(property.Name, property.GetValue(obj));
            }

            return (ExpandoObject)expando;
        }
    }
}
