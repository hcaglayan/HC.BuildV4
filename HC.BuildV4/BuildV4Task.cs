using HC.BuildV4.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HC.BuildV4
{
    /// <summary>
    /// Provides a way for multiple BuildV3 projects to build against a single target
    /// </summary>
    public class BuildV4Task : Microsoft.Build.Utilities.Task
    {

        [Microsoft.Build.Framework.Required]
        public string WorkingDirectory { get; set; }
        

        /// <summary>
        /// Project defns YAML file. Relative or absolute path
        /// </summary>
        [Microsoft.Build.Framework.Required]
        public string ProjectDefns { get; set; }

        /// <summary>
        /// Path to the Restored Artefacts. Relative or absolute
        /// </summary>
        [Microsoft.Build.Framework.Required]
        public string RestoredArtefacts { get; set; }

        /// <summary>
        /// Path to the Produced Artefacts. Relative or absolute
        /// </summary>
        [Microsoft.Build.Framework.Required]
        public string ProducedArtefacts { get; set; }



        public override bool Execute()
        {
            if (!Directory.Exists(WorkingDirectory))
            {
                Log.LogError("WorkingDirectory '{0}' not found.", WorkingDirectory);
                return false;
            }

            if (!File.Exists(ProjectDefns))
            {
                Log.LogError("ProjectDefns YAML file '{0}' not found.", ProjectDefns);
                return false;
            }

            string restoredArtefactsPath;
            if (Path.IsPathRooted(RestoredArtefacts))
            {
                restoredArtefactsPath = RestoredArtefacts;
            }
            else
            {
                restoredArtefactsPath = Path.Combine(WorkingDirectory, RestoredArtefacts);
            }

            if (!Directory.Exists(restoredArtefactsPath))
            {
                Log.LogError("Restored Artefacts folder not found. Path '{0}'", restoredArtefactsPath);
                return false;
            }


            string producedArtefactsPath;
            if (Path.IsPathRooted(ProducedArtefacts))
            {
                producedArtefactsPath = ProducedArtefacts;
            }
            else
            {
                producedArtefactsPath = Path.Combine(WorkingDirectory, ProducedArtefacts);
            }

            
            string projectDefnsContent = File.ReadAllText(ProjectDefns);
            var projects = ProjectsFileReader.ReadProjectFilesFromYaml(projectDefnsContent);
            foreach (var project in projects)
            {
                if (project.Folder == null)
                {
                    project.Folder = project.Name;
                }

                string projectPath;
                if (Path.IsPathRooted(project.Folder))
                {
                    projectPath = project.Folder;
                }
                else
                {
                    projectPath = Path.Combine(WorkingDirectory, project.Folder);
                }
                Log.LogMessage("Project {0}. Path: '{1}'", project.Name, projectPath);

                string localBuildFile = Path.Combine(projectPath, "LocalBuild.proj");
                if (!File.Exists(localBuildFile))
                {
                    Log.LogError("LocalBuild file '{0}' not found", localBuildFile);
                    return false;
                }

                string wrapUpFile = Path.Combine(projectPath, "WrapUp.proj");
                if (!File.Exists(wrapUpFile))
                {
                    Log.LogError("WrapUp file '{0}' not found", wrapUpFile);
                    return false;
                }


                StringBuilder dependenciesBuilder = new StringBuilder();

                foreach (var dep in project.Uses)
                {
                    string target = Path.Combine(restoredArtefactsPath, dep);
                    if (!Directory.Exists(target))
                    {
                        Log.LogError("Dependency package at '{0}' not found", target);
                        return false;
                    }
                    dependenciesBuilder.AppendFormat("{0}={1}", dep, target);
                    dependenciesBuilder.AppendLine();
                }

                string dependenciesPath = Path.Combine(projectPath, "DEPENDENCIES.TXT");
                File.WriteAllText(dependenciesPath, dependenciesBuilder.ToString());
                Log.LogMessage("Wrote dependencies to '{0}'", dependenciesPath);

                // Now build this project's LocalBuild using MSBUILD

                if (!base.BuildEngine.BuildProjectFile(localBuildFile, null, null, null))
                {
                    return false;
                }

                if (!base.BuildEngine.BuildProjectFile(wrapUpFile, null, null, null))
                {
                    return false;
                }

                // Grab the files from Targets and shove them back to the publishing area
                string targetsPath = Path.Combine(projectPath, "Targets");
                if (!Directory.Exists(targetsPath))
                {
                    Log.LogError("Project {0} has no artefacts produced at '{1}'", project.Name, targetsPath);
                    return false;
                }

                string publishToTarget = Path.Combine(producedArtefactsPath, project.Name);
                CopyDirectory(targetsPath, "*.*", true, publishToTarget);

                Log.LogMessage("Project {0} published to path: '{1}'", project.Name, publishToTarget);

            }

            return true;
        }


        /// <summary>
        /// Helper to copy entire directories
        /// </summary>
        private void CopyDirectory(string sourceDirectory, string searchPattern, bool includeSub, string destinationDirectory)
        {
            DirectoryInfo directory = new DirectoryInfo(sourceDirectory);
            FileInfo[] files = directory.GetFiles(searchPattern, (includeSub) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (FileInfo fi in files)
            {
                string sourceDirectoryForFile = fi.DirectoryName;
                string targetDirectoryForFile;

                if (sourceDirectoryForFile.ToLower().StartsWith(sourceDirectory.ToLower()))
                {
                    targetDirectoryForFile = sourceDirectoryForFile;
                    targetDirectoryForFile = targetDirectoryForFile.Remove(0, sourceDirectory.Length);
                    while (targetDirectoryForFile.Length > 0 && Path.IsPathRooted(targetDirectoryForFile))
                    {
                        targetDirectoryForFile = targetDirectoryForFile.Remove(0, 1);
                    }
                    targetDirectoryForFile = Path.Combine(destinationDirectory, targetDirectoryForFile);
                    string targetPath = Path.Combine(targetDirectoryForFile, fi.Name);

                    Directory.CreateDirectory(targetDirectoryForFile);
                    fi.CopyTo(targetPath);
                    File.SetAttributes(targetPath, File.GetAttributes(targetPath) & ~FileAttributes.ReadOnly);
                }
            }
        }
    }
}
