using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Helpers
{
    public static class Excel
    {
        public static DataSet ReadXls(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("File not found: " + filePath);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    return reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });
                }
            }
        }

        public static T Cast<T>(this object value)
        {
            var def = default(T);
            try
            {
                if (value == null || value is DBNull) return def;
                if (def is int) return (T)Convert.ChangeType(Convert.ToInt32(value), typeof(T));
                if (def is double) return (T)Convert.ChangeType(Convert.ToDouble(value), typeof(T));
                return (T)value;
            }
            catch (InvalidCastException ex)
            {
                Console.Error.WriteLine("Cast error: " + ex);
                return def;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Other error: " + ex);
                return def;
            }
        }
    }
}
