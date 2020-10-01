using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Build.Utilities;

namespace SourceLink.Common
{
    internal class SvnInfo
    {
        private readonly string _projectPath;
        private readonly TaskLoggingHelper _log;

        public string SvnRootUrl { get; private set; }
        public string SvnRevision { get; private set; }
        public string WorkingCopyPath { get; private set; }

        public SvnInfo(string projectPath, TaskLoggingHelper log)
        {
            _projectPath = projectPath;
            _log = log;

            Execute();
        }

        private void Execute()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_projectPath))
                {
                    _log.LogError("ProjectPath must not be null or empty/whitespace.");
                    return;
                }

                var info = ExecuteSvnInfoCommand();
                ParseInfo(info);
            }
            catch (XmlException e)
            {
                _log.LogMessage($"XmlException ({e.Message}) is caught, likely due to working directory not being SVN");
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _log.LogErrorFromException(e, true, true, null);
            }
        }

        private string ExecuteSvnInfoCommand()
        {
            var svnExe = FindSvnExe();
            if (!File.Exists(svnExe))
            {
                _log.LogError($"Fail to find {svnExe}");
                return null;
            }

            var ps = new System.Diagnostics.Process
                {
                    StartInfo =
                        {
                            Arguments = $"info --xml {_projectPath}",
                            CreateNoWindow = true,
                            ErrorDialog = false,
                            FileName = svnExe,
                            RedirectStandardInput = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                        }
                };

            using (ps)
            {
                if (!ps.Start())
                {
                    _log.LogError($"Failed to run {svnExe}");
                    return null;
                }

                ps.WaitForExit();

                var stdout = ps.StandardOutput.ReadToEnd();
                _log.LogMessage($"stdout of {svnExe}:");
                _log.LogMessage(stdout);

                var stderr = ps.StandardError.ReadToEnd();
                _log.LogMessage($"stderr of {svnExe}:");
                _log.LogMessage(stderr);

                ps.Close();
                return stdout;
            }
        }

        private static string FindSvnExe()
        {
            var svnPartialPath = Environment.Is64BitOperatingSystem ? "../x64/svn.exe" : "../x86/svn.exe";

            var executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
            return Path.Combine(Path.GetDirectoryName(executingAssemblyLocation), svnPartialPath);
        }

        private void ParseInfo(string info)
        {
            using (var reader = new StringReader(info))
            {
                var document = XDocument.Load(reader);
                var url = document.XPathSelectElement("//info/entry/url")?.Value;
                if (string.IsNullOrWhiteSpace(url))
                {
                    _log.LogError($"Failed to extract URL from 'svn info --xml' output.");
                    return;
                }

                SvnRevision = document.XPathSelectElement("//info/entry")?.Attribute("revision")?.Value;
                if (string.IsNullOrWhiteSpace(SvnRevision))
                {
                    _log.LogError($"Failed to extract 'revision id' from 'svn info --xml' output.");
                    return;
                }

                WorkingCopyPath = document.XPathSelectElement("//info/entry/wc-info/wcroot-abspath")?.Value?
                    .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                if (string.IsNullOrWhiteSpace(WorkingCopyPath))
                {
                    _log.LogError($"Failed to extract 'working copy path' from 'svn info --xml' output.");
                    return;
                }

                if (!_projectPath.StartsWith(WorkingCopyPath, StringComparison.CurrentCultureIgnoreCase))
                {
                    _log.LogError($"Project path ({_projectPath}) does not start with mapped path ({WorkingCopyPath}) as expected.");
                    return;
                }

                var localPath = _projectPath.Substring(WorkingCopyPath.Length).Replace(Path.DirectorySeparatorChar, '/');

                if (!url.EndsWith(localPath, StringComparison.CurrentCultureIgnoreCase))
                {
                    _log.LogError($"URL ({url}) does not end with local path ({localPath}) as expected.");
                    return;
                }

                SvnRootUrl = url.Substring(0, url.Length - localPath.Length);
            }
        }
    }
}