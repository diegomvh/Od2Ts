using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ODataApiGen.Models;

namespace ODataApiGen.Angular
{
    public class Collection : Structured
    {
        public Angular.Model Model { get; private set; }
        public Collection(StructuredType type, Angular.Model model) : base(type)
        {
            this.Model = model;
            this.Dependencies.Add(model);
            model.SetCollection(this);
        }
        
        // Imports
        public override IEnumerable<string> ImportTypes
        {
            get
            {
                var parameters = new List<Models.Parameter>();
                foreach (var cal in this.EdmStructuredType.Actions)
                    parameters.AddRange(cal.Parameters);
                foreach (var cal in this.EdmStructuredType.Functions)
                    parameters.AddRange(cal.Parameters);

                var list = new List<string> {
                    this.Model.EntityType
                };
                list.AddRange(parameters.Select(p => p.Type));
                list.AddRange(this.EdmStructuredType.Actions.SelectMany(a => this.CallableNamespaces(a)));
                list.AddRange(this.EdmStructuredType.Functions.SelectMany(a => this.CallableNamespaces(a)));
                list.AddRange(this.EdmStructuredType.Properties.Select(a => a.Type));
                if (this.EdmStructuredType is EntityType)
                    list.AddRange((this.EdmStructuredType as EntityType).NavigationProperties.Select(a => a.Type));
                return list;
            }
        }
        // Exports
        public override string FileName => this.EdmStructuredType.Name.ToLower() + ".collection";
        public override string Name => this.EdmStructuredType.Name + "Collection";
        public string BaseName => this.EdmStructuredType.Name + "BaseCollection";
        public string ModelName => this.Model.Name;
        public override IEnumerable<string> ExportTypes => new string[] { this.Name, this.BaseName };
        public override IEnumerable<Import> Imports => GetImportRecords();

        public override object ToLiquid()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Actions {
            get {
                var collectionActions = this.EdmStructuredType.Actions.Where(a => a.IsCollection);
                return collectionActions.Count() > 0 ? this.RenderCallables(collectionActions) : Enumerable.Empty<string>();
            }
        }
        public IEnumerable<string> Functions {
            get {
                var collectionFunctions = this.EdmStructuredType.Functions.Where(a => a.IsCollection);
                return collectionFunctions.Count() > 0 ? this.RenderCallables(collectionFunctions) : Enumerable.Empty<string>();
            }
        }

        public override IEnumerable<StructuredProperty> Properties => Enumerable.Empty<StructuredProperty>();
    }
}