using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace HC.BuildV4.Model
{
    public class Project
    {
        [YamlMember(Alias = "name", ApplyNamingConventions = false)]
        public string Name { get; set; }

        [YamlMember(Alias = "folder", ApplyNamingConventions = false)]
        public string Folder { get; set; }

        [YamlMember(Alias = "uses", ApplyNamingConventions = false)]
        public List<string> Uses { get; set; } = new List<string>();
    }
}
