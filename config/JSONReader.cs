using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;

namespace Lytheria.config
{
    internal class JSONReader
    {
        //reads json file
        public string token { get; set; }
        public string prefix { get; set; }

        public async Task ReadJSON()
        {
            // Reads the JSON config
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);

                this.token = data.token;
                this.prefix = data.prefix;
            }
        }
    }

    internal sealed class JSONStructure
    {
        public string token { get; set; }
        public string prefix { get; set; }
    }
}
