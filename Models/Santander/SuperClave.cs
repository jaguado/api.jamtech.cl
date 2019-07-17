using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JAMTech.Extensions;

namespace JAMTech.Models.Santander
{
    public class SuperClave
    {
        const string _localPath = @"App_Data\Keys\Santander";
        public string Cuenta { get; set; }
        public int Id { get; set; }
        public int[][] Keys;

        public SuperClave(string cuenta, int id, bool automaticallyLoadKeys=false)
        {
            Cuenta = cuenta;
            Id = id;
            if(automaticallyLoadKeys)
                Open();
        }

        internal void SaveLocalCopy()
        {
            File.WriteAllText(Path.Combine(_localPath, Cuenta + ".keys"), ToString());
        }
        protected void Open()
        {
            //find keys on environment variable, folder, etc...
            var path = Path.Combine(_localPath, Cuenta + ".keys");
            if(File.Exists(path))
                FromBase64String(Cuenta, Id, File.ReadAllText(path));
            else
            {
                //environment variables
                var keys = Environment.GetEnvironmentVariable("keys_santander_" + Cuenta);
                if (!string.IsNullOrEmpty(keys))
                    FromBase64String(Cuenta, Id, keys);
                else
                    throw new ApplicationException($"Keys of account '{Cuenta}' not found");
            }
        }

        public string GetKey(string position)
        {
            if (position.Length != 2) throw new ArgumentOutOfRangeException("Invalid position");
            var column = char.ToUpperInvariant(position[0]) - 65;
            var row = int.Parse(position[1].ToString()) - 1;
            if(row > Keys.Length || column > Keys[0].Length)
                throw new ArgumentOutOfRangeException("Position not found");
            return Keys[row][column].ToString().PadLeft(2, '0');
        }
        public string GetKeys(string position) => position.Split(';').Select(pos => GetKey(pos)).JoinString(";");
        public override string ToString() => Helpers.Cipher.Encrypt(JsonConvert.SerializeObject(Keys), Id.ToString());
        public override bool Equals(object obj) => ToString() == obj.ToString();
        public static SuperClave FromBase64String(string cuenta, int id,
            string base64String) => new SuperClave(cuenta, id) { Keys = JsonConvert.DeserializeObject<int[][]>(Helpers.Cipher.Decrypt(base64String, id.ToString())) };
    }
}
