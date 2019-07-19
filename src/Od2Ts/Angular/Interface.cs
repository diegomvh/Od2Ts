using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Od2Ts.Abstracts;
using Od2Ts.Models;

namespace Od2Ts.Angular {
    public class Interface : Renderable {
        public StructuredType EdmStructuredType {get; private set;}

        public Interface Base {get; private set;}

        public Interface(StructuredType type) {
            EdmStructuredType = type;
        }

        public void SetBase(Interface b) {
            this.Base = b;
        }

        public override string Render() {
            var properties = new List<string>();
            properties.AddRange(this.EdmStructuredType.Properties.Select(prop =>
                $"{prop.Name}" + (prop.IsRequired ? ":" : "?:") + $" {this.GetTypescriptType(prop.Type)};")
            );
            properties.AddRange(this.EdmStructuredType.NavigationProperties.Select(prop =>
                $"{prop.Name}" + 
                (prop.IsRequired ? ":" : "?:") + 
                $" {this.GetTypescriptType(prop.Type)}" + (prop.IsCollection ? "[];" : ";"))
            );

            var imports = this.RenderImports();
            return $@"{String.Join("\n", imports)}

export {this.GetSignature()} {{
  {String.Join("\n  ", properties)}
}}"; 
        }

        public string GetSignature() {
            var signature = $"interface {this.Name}";
            if (this.Base != null)
                signature = $"{signature} extends {this.Base.Name}";
            return signature;
        }

        private Uri _uri;

        public override string Name => this.EdmStructuredType.Name;
        public override string FileName => this.EdmStructuredType.Name.ToLower() + ".interface";
        public override string Directory => this.EdmStructuredType.NameSpace.Replace('.', Path.DirectorySeparatorChar);
        public override IEnumerable<string> Types
        {
            get
            {
                var types = this.EdmStructuredType.NavigationProperties
                    .Select(a => a.Type)
                    .Where(a => a != this.EdmStructuredType.Type)
                    .ToList();
                /*For Not-EDM types (e.g. enums with namespaces, complex types*/
                types.AddRange(this.EdmStructuredType.Properties
                    .Where(a => !a.IsEdmType)
                    .Select(a => a.Type));
                if (this.Base != null)
                    types.Add(this.Base.EdmStructuredType.Type);
                return types.Distinct();
            }
        }

    }
}