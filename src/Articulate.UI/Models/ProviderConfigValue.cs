using System;

namespace Articulate.Models
{
    public class ProviderConfigValue
    {
        public Type Provider { get; set; }
        public string File { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public int Index { get; set; }
    }
}