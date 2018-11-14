using System;
using System.IO;
using HC.BuildV4.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HC.BuildV4.Tests
{
    [TestClass]
    public class ProjectsFileReaderTests
    {
        private const string sampleProjectsFile = @"---
             - name: TestProject
               uses: 
                - SomeOtherProject

             - name: TestProject2
               folder: here
               uses: 
                - SomeOtherProject2
...";

        [TestMethod]
        public void ValidSample_AsExpected()
        {
            var projects = ProjectsFileReader.ReadProjectFilesFromYaml(sampleProjectsFile);

            Assert.IsNotNull(projects);
            Assert.IsTrue(projects.Length > 0);

            Assert.AreEqual(2, projects.Length);
            Assert.AreEqual("TestProject", projects[0].Name);
            Assert.IsNull(projects[0].Folder);
            Assert.IsTrue(projects[0].Uses.Count > 0);
            Assert.AreEqual("SomeOtherProject", projects[0].Uses[0]);

            Assert.AreEqual("TestProject2", projects[1].Name);
            Assert.AreEqual("here", projects[1].Folder);
            Assert.IsTrue(projects[1].Uses.Count > 0);
            Assert.AreEqual("SomeOtherProject2", projects[1].Uses[0]);


        }
    }
}
