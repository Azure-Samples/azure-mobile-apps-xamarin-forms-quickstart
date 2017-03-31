using System;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace TaskList
{
    public class TodoItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [Version]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "complete")]
        public bool Done { get; set; }
    }
}
