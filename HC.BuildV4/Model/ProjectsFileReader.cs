using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HC.BuildV4.Model
{
    public static class ProjectsFileReader
    {
        public static Project[] ReadProjectFilesFromYaml(string yamlFile)
        {
            var input = new StringReader(yamlFile);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var projects = deserializer.Deserialize<Project[]>(input);
            return projects;
        }
    }
}
