﻿using System;
using System.Linq;

namespace ODataApiGen.Models
{
    public class Property
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsEdmType { get { return !System.String.IsNullOrWhiteSpace(Type) && Type.StartsWith("Edm."); } }
        public bool Collection { get; set; }
        public bool Nullable { get; set; }
        public string MaxLength { get; set; }
        public string DisplayName { get; set; }
        public string SRID { get; set; }
    }
    public class PropertyRef
    {
        public string Name { get; set; }
        public string Alias { get; set; }
    }
}
