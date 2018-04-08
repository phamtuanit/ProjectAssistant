using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectAssistant.Platform.Model;

namespace ProjectAssistant.Platform.Helper
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using log4net;

    public static class MSBuildHelper
    {
        private const string MSBuildPath = @"C:\Program Files (x86)\MSBuild\14.0\Bin";

        private const string MSBuildFile = "MSBuild.exe";

        private const string NugetFile = "Nuget.exe";

        private const string NNuspecFile = "*.nuspec";

        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MSBuildHelper));

        /// <summary>
        /// Build nuget package
        /// </summary>
        /// <param name="nugetToolDir"></param>
        /// <param name="nugetOutput"></param>
        /// <param name="project"></param>
        internal static void BuildNuget(string nugetToolDir, string nugetOutput, ProjectInfo<ReferAssemblyInfo> project)
        {
            // Clear nuget lib
            var rootProject = Path.GetDirectoryName(project.Path);
            var nugetLib = Path.Combine(rootProject, @"NugetAssets\Lib");
            if (Directory.Exists(nugetLib))
            {
                Directory.Delete(nugetLib, true);
                Directory.CreateDirectory(nugetLib);
            }

            // Build project
            BuildProject(project.Path);

            // Make nuget package
            MakeNugetPackage(nugetToolDir, nugetOutput, project.Path);
        }

        private static void MakeNugetPackage(string nugetToolDir, string nugetOutput, string projectPath)
        {
            Logger.Debug($"[MakeNugetPackage] Build project: {projectPath}...");
            var rootProjectPath = Path.Combine(Path.GetDirectoryName(projectPath), "NugetAssets");
            var nuspecFiles = Directory.GetFiles(rootProjectPath, NNuspecFile, SearchOption.AllDirectories);

            if (!nuspecFiles.Any())
            {
                Logger.Warn($"[MakeNugetPackage] Nuspec could not be found at [{rootProjectPath}].");
                throw new Exception("Nuspec could not be found.");
            }

            var nuspecFile = nuspecFiles.First();
            var args = GetNugetPackArgs(nuspecFile);

            var nugetTool = Path.Combine(nugetToolDir, NugetFile);

            if (!File.Exists(nugetTool))
            {
                throw new Exception("Nuget tool dir could not be found.");
            }


            Logger.Debug($"[MakeNugetPackage] Build arg: {args}");
            var startInfo = new ProcessStartInfo
            {
                FileName = nugetTool,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = args,
                WorkingDirectory = nugetOutput,
                Verb = "runas"
            };

            if (!Directory.Exists(nugetOutput))
            {
                Directory.CreateDirectory(nugetOutput);
            }

            using (var process = new Process())
            {
                process.EnableRaisingEvents = true;
                process.StartInfo = startInfo;

                // Register data event to show more information
                process.OutputDataReceived += OnDataReceived;
                process.ErrorDataReceived += OnDataReceived;

                // Run process
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                process.OutputDataReceived -= OnDataReceived;
                process.ErrorDataReceived -= OnDataReceived;

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Unknown exception with Exit code = {process.ExitCode}."); ;
                }
            }
            Logger.Debug($"[MakeNugetPackage] Build project: {projectPath}... DONE");
        }

        /// <summary>
        /// Build nuget pack arg
        /// </summary>
        /// <param name="projectPath"></param>
        /// <returns></returns>
        private static string GetNugetPackArgs(string projectPath)
        {
            // pack foo.csproj -Properties Configuration=Release
            return $@"pack ""{projectPath}"" -Properties Configuration=Release";
        }

        /// <summary>
        /// Build project
        /// </summary>
        /// <param name="projectPath"></param>
        internal static void BuildProject(string projectPath)
        {
            Logger.Debug($"[BuildProject] Build project: {projectPath}...");
            var args = GetBuildArgs(projectPath);

            var msBuildFile = Path.Combine(MSBuildPath, MSBuildFile);
            Logger.Debug($"[BuildProject] Build arg: {args}");
            var startInfo = new ProcessStartInfo
            {
                FileName = msBuildFile,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = args,
                Verb = "runas"
            };

            using (var process = new Process())
            {
                process.EnableRaisingEvents = true;
                process.StartInfo = startInfo;

                // Register data event to show more information
                process.OutputDataReceived += OnDataReceived;
                process.ErrorDataReceived += OnDataReceived;

                // Run process
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                process.OutputDataReceived -= OnDataReceived;
                process.ErrorDataReceived -= OnDataReceived;

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Unknown exception with Exit code = {process.ExitCode}.");;
                }
            }
            Logger.Debug($"[BuildProject] Build project: {projectPath}... DONE");
        }

        /// <summary>
        /// Get MSBuild arg
        /// </summary>
        /// <param name="projectPath"></param>
        /// <returns></returns>
        private static string GetBuildArgs(string projectPath)
        {
            return $@"""{projectPath}"" /property:Configuration=Release /t:rebuild /m /p:Platform=x86";
        }

        /// <summary>
        /// Called when [data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DataReceivedEventArgs"/> instance containing the event data.</param>
        private static void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                Logger.Info($"Build output: {e.Data}.");
            }
        }
    }
}
