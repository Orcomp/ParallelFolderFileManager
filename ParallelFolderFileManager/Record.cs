

namespace ParallelFolderFileManager
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    
    public class Record
    {
        public Record()
        {
            Children = new List<Record>();
        }
        
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("selection")]
        public string Selection { get; set; }
        [JsonPropertyName("line")]
        public int Line { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("otherJson")]
        public string OtherJson { get; set; }
        [JsonPropertyName("expanded")]
        public bool Expanded { get; set; }
        [JsonPropertyName("children")]
        public List<Record> Children { get; set; }
    }
}