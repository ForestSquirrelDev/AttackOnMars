using System;

namespace Utils.HelperAttributes {
    public class OffMainThreadUsageAttribute : Attribute {
        public OffMainThreadUsage Usage { get; }
        
        public OffMainThreadUsageAttribute(OffMainThreadUsage usage) {
            Usage = usage;
        }
    }
    public enum OffMainThreadUsage {
        Allowed = 0, Disallowed = 1
    }
}

