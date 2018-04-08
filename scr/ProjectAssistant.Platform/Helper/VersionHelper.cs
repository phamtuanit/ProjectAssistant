namespace ProjectAssistant.Platform.Helper
{
    using System;

    /// <summary>
    /// The VersionHelper class
    /// </summary>
    public static class VersionHelper
    {
        /// <summary>
        /// Increases the version.
        /// </summary>
        /// <param name="assVersion">The ass version.</param>
        /// <param name="incretionStep">The incretion step.</param>
        /// <param name="versionNum">The number of version: 3 or 4.</param>
        /// <returns>New version</returns>
        public static string GetIncreasionVersion(string assVersion, int incretionStep = 1, int versionNum = 4)
        {
            var verArr = assVersion.Split('.');
            verArr[versionNum - 1] = ((int.Parse(verArr[versionNum - 1])) + incretionStep).ToString();
            return string.Join(".", verArr);
        }

        /// <summary>
        /// Reads the assembly version.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public static string ReadAsemblyVersion(string value)
        {
            var startIndex = value.IndexOf("\"", StringComparison.Ordinal);
            var endIndex = value.IndexOf("\"", startIndex + 1, StringComparison.Ordinal);
            var version = value.Substring(startIndex + 1, endIndex - startIndex - 1);

            return version;
        }
    }
}