using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ConsoleApp;
using ConsoleApp.Validator;
using Exy;
using FilesCleanUp.Validator;

namespace FilesCleanUp {
    public class ConsoleInterface {
        const String Title = "Files clean-up tool";

        public void Run() {
            try {
                String response = Initialize();
                Boolean proceed = HandleResponse(response);

                while (proceed) {
                    Process();

                    Console.WriteLine(new StringBuilder()
                        .AppendLine()
                        .Append($"{Title}. Enter to continue. C, E or Q to exit.")
                        .ToString());

                    proceed = HandleResponse(Console.ReadLine());
                }

                HandleExit();
            }
            catch (Exception ex) {
                HandleError(ex);
            }
        }

        public String Initialize() {
            Console.Title = Title;

            Console.WriteLine(new StringBuilder()
                .AppendLine()
                .Append($"{Title}. Enter to continue. C, E or Q to exit.")
                .ToString());

            return Console.ReadLine();
        }

        readonly IDictionary<String, Action> modeAction = new Dictionary<String, Action> {
            ["l"] = ListAction,
            ["d"] = DeleteAction
        };

        public void Process() {
            String mode = ExtConsole
                .Create()
                .LabelWith("Mode: ([L]ist, [D]elete) ")
                .GetString(new ModeValidator("Choose one: L, D"));

            modeAction[mode.ToLowerInvariant()]();

            Console.WriteLine(new StringBuilder()
                .AppendLine("Done.")
                .AppendLine()
                .ToString());
        }

        static void ListAction() {
            String profile = ExtConsole
                .Create()
                .LabelWith("Profile: ")
                .GetString(new SimpleStringValidator("Select profile from config"));

            ListFiles(profile);
        }

        static void ListFiles(String profile) {
            XmlDocument config = LoadFromPath($"{AppDomain.CurrentDomain.BaseDirectory}\\config.xml");
            String profileSelector = $"Configuration/Profile[@Name='{profile}']";
            XmlNode profileConfig = config.SelectSingleNode(profileSelector);
            if (profileConfig == null) {
                IEnumerable<String> profNames = config
                    .SelectNodes("Configuration/Profile")
                    .Cast<XmlNode>()
                    .Select(profNode => GetAttributeValue(profNode, "Name"));

                throw new InvalidOperationException($"Profile '{profile}' not found. Existing were {String.Join(", ", profNames)}");
            }

            Console.WriteLine("  > Reading Config.");

            IEnumerable<String> rootDirs = config
                .SelectNodes($"{profileSelector}/RootDir")
                .Cast<XmlNode>()
                .Select(dirNode => dirNode.InnerText);

            IEnumerable<String> includes = config
                .SelectNodes($"{profileSelector}/Include")
                .Cast<XmlNode>()
                .Select(incNode => incNode.InnerText);

            IEnumerable<String> excludes = config
                .SelectNodes($"{profileSelector}/Exclude")
                .Cast<XmlNode>()
                .Select(excNode => excNode.InnerText);

            Boolean.TryParse(
                GetAttributeValue(
                    config.SelectSingleNode($"{profileSelector}/GenerateFile"), "IncludeList"), out Boolean generateIncludeList);

            Boolean.TryParse(
                GetAttributeValue(
                    config.SelectSingleNode($"{profileSelector}/GenerateFile"), "ExcludeList"), out Boolean generateExcludeList);

            Console.WriteLine("  > Done.");

            Console.WriteLine("  > List directory.");

            String includedListFilename = $"{AppDomain.CurrentDomain.BaseDirectory}\\IncludedList-{DateTime.Now:yyyyMMddhhmmss}.log";
            String excludedListFilename = $"{AppDomain.CurrentDomain.BaseDirectory}\\ExcludedList-{DateTime.Now:yyyyMMddhhmmss}.log";
            String finalListFilename = $"{AppDomain.CurrentDomain.BaseDirectory}\\FinalList-{DateTime.Now:yyyyMMddhhmmss}.log";

            foreach (String rootDir in rootDirs) {
                Console.WriteLine($"    > {rootDir}");

                IList<String> files = Directory
                    .GetFiles(rootDir, "*.*", SearchOption.AllDirectories)
                    .Where(file => includes.Aggregate(false, (acc, curr) => acc || Regex.IsMatch(file, curr)))
                    .ToList();

                if (generateIncludeList)
                    File.AppendAllText(includedListFilename, String.Join(Environment.NewLine, files));

                IList<String> excluded = files
                    .Where(file => excludes.Aggregate(false, (acc, curr) => acc || Regex.IsMatch(file, curr)))
                    .ToList();

                if (generateExcludeList)
                    File.AppendAllText(excludedListFilename, String.Join(Environment.NewLine, excluded));

                IList<String> final = files
                    .Where(file => excludes.Aggregate(true, (acc, curr) => acc && !Regex.IsMatch(file, curr)))
                    .ToList();

                File.AppendAllText(finalListFilename, String.Join(Environment.NewLine, final));
            }

            Console.WriteLine("  > Done.");
        }

        static void DeleteAction() {
            Console.WriteLine("  > Reading list files.");

            IList<String> finalListFiles = Directory
                .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "FinalList-*.log", SearchOption.TopDirectoryOnly)
                .ToList();

            if (!finalListFiles.Any())
                throw new InvalidOperationException("Generate list first.");

            if (finalListFiles.Count > 1)
                throw new InvalidOperationException("More than 1 list files detected. Please choose 1 and delete the others.");

            String finalListFile = finalListFiles.First();
            IEnumerable<String> lines = File.ReadAllLines(finalListFile);
            if (!lines.Any())
                throw new InvalidOperationException("File is empty.");

            Console.WriteLine("  > Done.");

            Console.WriteLine("  > Deleting.");

            foreach (String toBeDeletedPath in lines) {
                if (File.Exists(toBeDeletedPath))
                    File.Delete(toBeDeletedPath);

                String parentPath = Path.GetDirectoryName(toBeDeletedPath);
                if (Directory.Exists(parentPath)) {
                    IEnumerable<String> files = Directory.GetFiles(parentPath, "*.*", SearchOption.AllDirectories);
                    if (!files.Any())
                        Directory.Delete(parentPath);
                }
            }

            Console.WriteLine("  > Done.");
        }

        public Boolean HandleResponse(String response) {
            String[] quitCommands = { "c", "e", "q" };
            return !quitCommands.Contains(response.ToLowerInvariant());
        }

        public void HandleExit() {
            Console.WriteLine(new StringBuilder()
                .AppendLine("Exit.")
                .AppendLine()
                .ToString());

            Console.ReadLine();
        }

        public void HandleError(Exception ex) {
            Console.WriteLine(new StringBuilder()
                .AppendLine("Error:")
                .AppendLine(ex.GetExceptionMessage())
                .AppendLine()
                .ToString());

            Console.ReadLine();
        }

        static XmlDocument LoadFromPath(String path) {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            String content = File.ReadAllText(path);
            return Load(content);
        }

        static XmlDocument Load(String xml) {
            if (String.IsNullOrEmpty(xml) || String.IsNullOrEmpty(xml.Trim()))
                return null;

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            return xmlDoc;
        }

        static XmlNode GetAttribute(XmlNode node, String name) {
            if (node != null && node.Attributes != null) {
                XmlAttribute attr = node.Attributes[name];
                if (attr != null)
                    return (XmlNode) attr;
            }

            return null;
        }

        static String GetAttributeValue(XmlNode node, String name) {
            XmlNode attr = GetAttribute(node, name);
            if (attr == null)
                return String.Empty;

            return attr.Value;
        }
    }
}
