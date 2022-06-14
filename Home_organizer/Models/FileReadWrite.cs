using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_organizer.Models
{
    class FileReadWrite
    {
        private readonly string path;

        public FileReadWrite(string _path)
        {
            path = _path;
        }

        public List<User> LoadData()
        {
            if (!File.Exists(path))
            {
                return new List<User>();
            }
            using(var reader = File.OpenText(path))
            {
                var fileText = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<User>>(fileText);
            }
        }
        public void SaveData(List<User> _listUsers)
        {
            using(StreamWriter writer = File.CreateText(path))
            {
                writer.Write(JsonConvert.SerializeObject(_listUsers));
            }
        }
    }
}
