using System;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Xunit.Abstractions;
using System.IO;
using HC.BuildV4.Model;

namespace HC.BuildV4.Tests
{
    public class UnitTest1
    {
        private const string sampleProjectFile = @"---
             - name: TestProject
               uses: 
                - SomeOtherProject

             - name: TestProject2
               folder: here
               uses: 
                - SomeOtherProject2
...";

        [Fact]
        public void ValidSample_AsExpected()
        {
            var input = new StringReader(sampleProjectFile);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var projects = deserializer.Deserialize<Project[]>(input);

            Assert.NotNull(projects);
            Assert.NotEmpty(projects);

            Assert.Equal(2, projects.Length);
            Assert.Equal("TestProject", projects[0].Name);
            Assert.Null(projects[0].Folder);
            Assert.NotEmpty(projects[0].Uses);
            Assert.Equal("SomeOtherProject", projects[0].Uses[0]);

            Assert.Equal("TestProject2", projects[1].Name);
            Assert.Equal("here", projects[1].Folder);
            Assert.NotEmpty(projects[1].Uses);
            Assert.Equal("SomeOtherProject2", projects[1].Uses[0]);


        }
    }
}
