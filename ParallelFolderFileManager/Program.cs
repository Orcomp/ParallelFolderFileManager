namespace ParallelFolderFileManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    class Program
    {
        static void Main(string[] args)
        {
            var projectFolder = @"C:\Source\SES.Projects.GFG\src\SES.Projects.GFG.Chameleon.NetworX";
            var removeRootPath = @"C:\Source\SES.Projects.GFG\src\";
            
            // var jsonFilePathRead = @"C:\Source\SES.Projects.GFG\src\parallelFolders.json";
            var jsonFilePathWrite = @"C:\Source\SES.Projects.GFG\src\parallelFolders.json";

            var sw = Stopwatch.StartNew();

            // Get all .cs and .xaml files from the project
            var files = GetFilePaths(projectFolder);

            var fileNames = new Dictionary<string, string>();
            var recordsByGroup = new Dictionary<string, Record>();

            var modelNames = new Dictionary<string, string>();

            foreach (var file in files)
            {
                var filePath = file.Replace(removeRootPath, "");
                
                var fileInfo = new FileInfo(file);

                var fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                
                if(fileNames.ContainsKey(fileName))
                {
                    continue;
                }

                fileNames.Add(fileName, filePath);

                // Get all the files that are found in the "Model" folder
                if (fileInfo.DirectoryName.Contains("Model") && !fileInfo.DirectoryName.Contains("ViewModel"))
                {
                    modelNames.Add(fileName, filePath);
                }

                //-------------------------------------------------
                // Check if file has defined groups in header
                // i.e. look for tags

                var tagsLine = File.ReadLines(file).Take(10).FirstOrDefault(x => x.Contains("#"));

                if (!string.IsNullOrEmpty(tagsLine))
                {
                    // Get groups (i.e. tags)
                    var groupsText = tagsLine.Replace(@"//", "").Replace("#", "");
                    var groups = groupsText.Split(',');

                    foreach (var group in groups)
                    {
                        var groupName = group.Trim();

                        if (string.IsNullOrWhiteSpace(groupName))
                        {
                            continue;
                        }

                        if (!recordsByGroup.ContainsKey(groupName))
                        {
                            var recordGroup = new Record
                            {
                                Name = groupName,
                            };
                            
                            recordsByGroup.Add(groupName, recordGroup);
                        }

                        var record = new Record
                        {
                            Name = fileName,
                            Path = filePath,
                        };

                        recordsByGroup[groupName].Children.Add(record);
                    }
                }
            }

            //====================================================
            // Create groups from model names
            // and find matching files

            foreach (var modelName in modelNames)
            {
                if (!recordsByGroup.ContainsKey(modelName.Key))
                {
                    var recordGroup = new Record
                    {
                        Name = modelName.Key,
                    };
                    
                    recordsByGroup.Add(modelName.Key, recordGroup);
                }

                // Find other files that contain the model name in them
                var matchingRecords = fileNames.Where(x => x.Key.Contains(modelName.Key) && !x.Equals(modelName))
                    .Select(x => new Record {Name = x.Key, Path = x.Value});

                recordsByGroup[modelName.Key].Children.AddRange(matchingRecords);
            }
            
            //====================================================
            // Export to json
            
            Serialise(recordsByGroup.Values.Where(x => x.Children.Any()).ToList(), jsonFilePathWrite);
            
            //====================================================

            sw.Stop();
            var elapsedSeconds = sw.Elapsed.Milliseconds / 1000d;

            Console.WriteLine($"Finished processing {fileNames.Count} files in {elapsedSeconds:N3} secs");
        }

        public static IEnumerable<string> GetFilePaths(string dir)
        {
            var options = new EnumerationOptions();
            options.RecurseSubdirectories = true;
            options.ReturnSpecialDirectories = false;
            //opt.AttributesToSkip = FileAttributes.Hidden | FileAttributes.System;
            options.AttributesToSkip = 0;
            options.IgnoreInaccessible = true;

            var filePaths = Directory.EnumerateFileSystemEntries(dir, "*", options)
                                    .Where(x => (x.EndsWith(".cs") || x.EndsWith(".xaml")) &&
                                                !x.Contains(@"\obj\") &&
                                                !x.EndsWith(".xaml.cs")
                                           );
            
            
            return filePaths;
        }
        
        public static void Serialise(List<Record> records, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var text = JsonSerializer.Serialize(records, options);
            
            File.WriteAllText(filePath, text);
        }

        public static List<Record> Deserialise(string filePath)
        {
            var json = File.ReadAllText(filePath);
            
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
            };
            
            return JsonSerializer.Deserialize<List<Record>>(json, options);
        }
    }
}