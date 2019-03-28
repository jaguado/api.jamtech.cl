using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Helpers
{
    public static class Sii
    {
        private static IEnumerable<Models.Vehicle> _siiDb = null;
        private static void LoadSiiVehiclesDB()
        {
            //load xls
            var book = Excel.ReadXls(Path.Combine("App_Data", "liv2019xls.xlsx"));
            if (book.Tables.Count > 0)
            {
                _siiDb = book.Tables[0].Select().Select(row => new Models.Vehicle
                {
                    Codigo = row.ItemArray[0].Cast<string>(),
                    CodigoAnterior = row.ItemArray[1].Cast<string>(),
                    Año = row.ItemArray[2].Cast<double>(),
                    Tipo = row.ItemArray[3].Cast<string>(),
                    Marca = row.ItemArray[4].Cast<string>(),
                    Modelo = row.ItemArray[5].Cast<string>(),
                    Version = row.ItemArray[6].Cast<string>(),
                    Puertas = row.ItemArray[7].Cast<double>(),
                    Cilindrada = row.ItemArray[8].Cast<double>(),
                    Potencia = row.ItemArray[9].Cast<double>(),
                    Combustible = row.ItemArray[10].Cast<string>(),
                    Transmision = row.ItemArray[11].Cast<string>(),
                    Tasacion = row.ItemArray[17].Cast<double>(),
                    Permiso = row.ItemArray[18].Cast<double>()
                });
            }
        }
        public static IEnumerable<Models.Vehicle> GetVehicleInfoFromSII(Models.Vehicle vehicle)
        {
            if (_siiDb == null)
                LoadSiiVehiclesDB();

            return _siiDb.Where(v => 
                                    (vehicle.Año == 00 || v.Año == vehicle.Año) &&
                                    (vehicle.Marca == null || v.Marca.Contains(vehicle.Marca, StringComparison.InvariantCultureIgnoreCase)) &&
                                    (vehicle.Modelo == null || v.Modelo.Contains(vehicle.Modelo, StringComparison.InvariantCultureIgnoreCase)) &&
                                    (vehicle.Cilindrada == 0 || v.Cilindrada == vehicle.Cilindrada) &&
                                    (vehicle.Potencia == 0 || v.Potencia == vehicle.Potencia) &&
                                    (vehicle.Combustible == null || v.Combustible.Contains(vehicle.Combustible, StringComparison.InvariantCultureIgnoreCase)) &&
                                    (vehicle.Transmision == null || v.Transmision.Contains(vehicle.Transmision, StringComparison.InvariantCultureIgnoreCase))
                                );
        }
    }
}
