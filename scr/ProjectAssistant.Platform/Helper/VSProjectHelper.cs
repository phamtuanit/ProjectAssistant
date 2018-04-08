namespace ProjectAssistant.Platform.Helper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Constant;
    using Model;
    using Contract.Model;

    public static class VSProjectHelper
    {
        /// <summary>
        /// Changes the reference version.
        /// </summary>
        /// <param name="projectFile">The project file.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        public static bool ChangeReferenceVersion(string projectFile, string assemblyName, string version)
        {
            const string xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var xmlDoc = new XmlDocument();
            XmlNamespaceManager nsMngr;

            using (var reader = new XmlTextReader(projectFile))
            {
                nsMngr = new XmlNamespaceManager(reader.NameTable);
                nsMngr.AddNamespace("a", xmlns);

                xmlDoc.Load(reader);
                reader.Dispose();
            }

            var nodes = xmlDoc.SelectNodes("//a:Reference", nsMngr);
            if (nodes != null)
            {
                foreach (XmlNode refElement in nodes)
                {
                    var includeAtt = refElement.Attributes["Include"];
                    var data = includeAtt.Value.Split(',');
                    var assName = data[0];

                    if (assName.Equals(assemblyName, StringComparison.CurrentCultureIgnoreCase) && data.Length >= 2)
                    {
                        var oldVersion = data[1].Split('=')[1];
                        if (string.Equals(oldVersion, version))
                        {
                            return false;
                        }
                        var newVersion = data[1].Replace(oldVersion, version);
                        
                        data[1] = newVersion;

                        var newIncludeData = String.Join(",", data);
                        includeAtt.Value = newIncludeData;

                        // Update project version
                        xmlDoc.Save(projectFile);
                        break;
                    }
                }
            }
            
            return true;
        }

        /// <summary>
        /// Gets the reference version.
        /// </summary>
        /// <param name="projectFile">The project file.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static string GetReferenceVersion(string projectFile, string assemblyName)
        {
            try
            {
                var oldVersion = string.Empty;
                const string xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
                var xmlDoc = new XmlDocument();
                XmlNamespaceManager nsMngr;

                using (var reader = new XmlTextReader(projectFile))
                {
                    nsMngr = new XmlNamespaceManager(reader.NameTable);
                    nsMngr.AddNamespace("a", xmlns);

                    xmlDoc.Load(reader);
                    reader.Dispose();
                }

                var nodes = xmlDoc.SelectNodes("//a:Reference", nsMngr);
                if (nodes != null)
                {
                    foreach (XmlNode refElement in nodes)
                    {
                        var includeAtt = refElement.Attributes?["Include"];
                        if (includeAtt != null)
                        {
                            var data = includeAtt.Value.Split(',');
                            var assName = data[0];

                            if (assName.Equals(assemblyName, StringComparison.CurrentCultureIgnoreCase) && data.Length >= 2)
                            {
                                oldVersion = data[1].Split('=')[1];
                                break;
                            }
                        }
                    }
                }

                return oldVersion;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Reads the nuget version.
        /// </summary>
        /// <param name="nugetInfo">The nuspec information.</param>
        public static void BuildNugetVersion<U>(this NugetInfo<U> nugetInfo)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(nugetInfo.Path);
            var xmlNodeList = xmlDoc.GetElementsByTagName("version");
            foreach (XmlNode node in xmlNodeList)
            {
                var version = node.FirstChild.Value;
                if (version != null)
                {
                    nugetInfo.NugetVersion = version;
                }
            }
        }

        /// <summary>
        /// Changes the dependent nuget version.
        /// </summary>
        /// <param name="refNugetName">Name of the reference nuget.</param>
        /// <param name="nuspecPath">The nuspec path.</param>
        /// <param name="versionInfo">The version information.</param>
        internal static void ChangeDependentNugetVersion(string refNugetName, string nuspecPath, VersionInfo versionInfo)
        {
            var newVersion = versionInfo.Version;
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(nuspecPath);
            var dependencyNodeList = xmlDoc.GetElementsByTagName("dependency");
            foreach (XmlNode node in dependencyNodeList)
            {
                var nugetName = node.Attributes["id"]?.Value;
                if (nugetName == refNugetName)
                {
                    node.Attributes["version"].Value = newVersion;
                    xmlDoc.Save(nuspecPath);
                }
            }
        }

        /// <summary>
        /// Changes the reference nuget version.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="refNugetName">Name of the reference nuget.</param>
        /// <param name="newVersion">The new version.</param>
        /// <returns></returns>
        internal static bool ChangeRefNugetVersion(string path, string refNugetName, string newVersion)
        {
            // Update package.config file
            var result = false;
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            var xmlNodeList = xmlDoc.GetElementsByTagName("package");
            foreach (XmlNode node in xmlNodeList)
            {
                var nugetName = node.Attributes["id"]?.Value;
                if (nugetName == refNugetName)
                {
                    node.Attributes["version"].Value = newVersion;
                    xmlDoc.Save(path);
                    result = true;
                }
            }

            // Continue update new version in *.csproj file
            if (result)
            {
                // Get directory
                var dirPath = Path.GetDirectoryName(path);

                // Get .csproj file
                if (dirPath != null)
                {
                    var csPrjFilePath = Directory.GetFiles(dirPath, "*.csproj", SearchOption.TopDirectoryOnly);
                    if (csPrjFilePath.Any())
                    {
                        // Get and set new value for Reference
                        UpdateNugetRefVersion(csPrjFilePath[0], refNugetName, newVersion);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the assembly information.
        /// </summary>
        /// <param name="projectInfo">The project information.</param>
        public static void GetAssemblyInfo(this ProjectInfo<ReferAssemblyInfo> projectInfo)
        {
            // Get AssemblyInfo.cs file
            var dirName = Path.GetDirectoryName(projectInfo.Path) + $@"\{FileVersionInfoConst.PropertiesFolderName}";

            if (!Directory.Exists(dirName))
            {
                return;
            }

            var files = Directory.GetFiles(dirName, FileVersionInfoConst.AssemblyFileName, SearchOption.TopDirectoryOnly);
            if (files.Any())
            {
                using (var reader = new StreamReader(files[0]))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("//"))
                        {
                            continue;
                        }

                        if (line.Contains(FileVersionInfoConst.AssemblyInformationalVersionConst))
                        {
                            projectInfo.InformationalVersion = VersionHelper.ReadAsemblyVersion(line);
                        }
                        else if (line.Contains(FileVersionInfoConst.AssemblyVersionConst))
                        {
                            projectInfo.AssemblyVersion = VersionHelper.ReadAsemblyVersion(line);
                        }
                        else if (line.Contains(FileVersionInfoConst.AssemblyFileVersionConst))
                        {
                            projectInfo.FileVersion = VersionHelper.ReadAsemblyVersion(line);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Increases the nuget version.
        /// </summary>
        /// <param name="nugetInfo">The nuspec information.</param>
        /// <param name="newVersion">The new version.</param>
        public static void IncreaseNugetVersion(this NugetInfo<RefNugetInfo> nugetInfo, string newVersion)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(nugetInfo.Path);
            var xmlNodeList = xmlDoc.GetElementsByTagName("version");
            foreach (XmlNode node in xmlNodeList)
            {
                var curVersion = node.FirstChild.Value;
                // var increasingVersion = VersionHelper.GetIncreasionVersion(curVersion, 1, 3);
                if (!string.IsNullOrEmpty(curVersion))
                {
                    nugetInfo.NugetVersion = newVersion;
                    node.FirstChild.Value = newVersion;
                }
            }

            // Save change 
            xmlDoc.Save(nugetInfo.Path);
        }

        /// <summary>
        /// Gets the reference nuget.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        internal static IList<string> GetRefNuget(string filePath)
        {
            var result = new List<string>();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            var xmlNodeList = xmlDoc.GetElementsByTagName("package");
            foreach (XmlNode node in xmlNodeList)
            {
                var nugetName = node.Attributes["id"];
                result.Add(nugetName.Value);
            }

            return result;
        }

        /// <summary>
        /// Equals the specified another list.
        /// </summary>
        /// <param name="sourceList">The source list.</param>
        /// <param name="anotherList">Another list.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool Equals(List<string> sourceList, List<string> anotherList)
        {
            if (sourceList.Count != anotherList.Count)
            {
                return false;
            }

            var count = 0;
            foreach (var srcItem in sourceList)
            {
                var isContain = anotherList.Any(n => n.Equals(srcItem));
                if (!isContain)
                {
                    return false;
                }
                count++;
            }
            if (count == sourceList.Count)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the reference nuget version.
        /// </summary>
        /// <param name="fullName">The full name.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        internal static string GetRefNugetVersion(string fullName, string assemblyName)
        {
            var result = new List<string>();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fullName);
            var xmlNodeList = xmlDoc.GetElementsByTagName("package");
            foreach (XmlNode node in xmlNodeList)
            {
                var id = node.Attributes["id"]?.Value;
                if (id == assemblyName)
                {
                    return node.Attributes["version"]?.Value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the reference version.
        /// </summary>
        /// <param name="projectFilePath">The project file.</param>
        /// <param name="nugetName">Name of the assembly.</param>
        /// <param name="newVersion">The new version.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static string UpdateNugetRefVersion(string projectFilePath, string nugetName, string newVersion)
        {
            try
            {
                var oldVersion = string.Empty;
                const string xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
                var xmlDoc = new XmlDocument();
                XmlNamespaceManager nsMngr = null;

                using (var reader = new XmlTextReader(projectFilePath))
                {
                    if (reader.NameTable != null)
                    {
                        nsMngr = new XmlNamespaceManager(reader.NameTable);
                        nsMngr.AddNamespace("a", xmlns);

                        xmlDoc.Load(reader);
                        reader.Dispose();
                    }
                }

                var nodes = xmlDoc.SelectNodes("//a:Reference", nsMngr);

                if (nodes == null)
                {
                    return oldVersion;
                }

                foreach (XmlNode refElement in nodes)
                {
                    var includeAtt = refElement.Attributes["Include"];
                    var data = includeAtt.Value.Split(',');
                    var assemblyName = data[0];
                    var childNodes = refElement.ChildNodes;
                    foreach (XmlNode childNode in childNodes)
                    {
                        if (!childNode.Name.Equals("HintPath"))
                        {
                            continue;
                        }

                        // Update value for this node in here
                        var nodeValue = childNode.InnerText;
                        var indexOfPackages = -1;
                        var splitedValue = nodeValue.Split('\\');

                        // Search the index of "packages" text  
                        for (var i = 0; i < splitedValue.Length; i++)
                        {
                            if (!splitedValue[i].Equals("packages"))
                            {
                                continue;
                            }
                            indexOfPackages = i;
                            break;
                        }

                        if (indexOfPackages == -1)
                        {
                            continue;
                        }
                        // Common.Logging 
                        if (assemblyName.Equals(nugetName))
                        {
                            // Update new value for HintPath node
                            var oldVersionStr = splitedValue[indexOfPackages + 1];
                            var indexOfFirstNumber = oldVersionStr.IndexOfAny("0123456789".ToCharArray());
                            if (indexOfFirstNumber != -1)
                            {
                                var restBeforeVersion = oldVersionStr.Substring(0, indexOfFirstNumber);
                                var newVersionStr = $"{restBeforeVersion}{newVersion}";

                                splitedValue[indexOfPackages + 1] = newVersionStr;
                                childNode.InnerText = string.Join(@"\", splitedValue);
                                xmlDoc.Save(projectFilePath);
                                break;
                            }
                        }
                    }
                }
                return oldVersion;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
