using System.Collections.Generic;
using System.Linq;
using System;
using ODataApiGen.Models;
using Newtonsoft.Json;

namespace ODataApiGen.Angular
{
    public class MetaField : StructuredProperty {
        protected IEnumerable<PropertyRef> Keys { get; set; }
        public MetaField(Models.Property property, IEnumerable<PropertyRef> keys) : base(property) {
            this.Keys = keys;
        }
        public override string Name => Value.Name;
        public override string Type { 
            get {
                var values = new Dictionary<string, string>();
                values.Add("type", $"'{AngularRenderable.GetType(this.Value.Type)}'");
                var key = this.Keys.FirstOrDefault(k => k.Name == this.Value.Name);
                if (key != null) {
                    values.Add("key", "true");
                    values.Add("ref", $"'{key.Name}'");
                    if (!String.IsNullOrWhiteSpace(key.Alias)) {
                        values.Add("name", $"'{key.Alias}'");
                    }
                }
                if (!(this.Value is NavigationProperty) && !this.Value.Nullable)
                    values.Add("nullable", "false");
                if (!String.IsNullOrEmpty(this.Value.MaxLength) && this.Value.MaxLength.ToLower() != "max")
                    values.Add("maxLength", this.Value.MaxLength);
                if (!String.IsNullOrEmpty(this.Value.SRID))
                    values.Add("srid", this.Value.SRID);
                if (this.Value.IsCollection)
                    values.Add("collection", "true");
                if (this.Value.IsEnumType) {
                    var enu = this.Value.ResolveType() as EnumType;
                    values.Add("flags", enu.Flags.ToString().ToLower());
                } else if (this.Value is NavigationProperty) {
                    // Is Navigation
                    values.Add("navigation", "true");
                    var nav = this.Value as NavigationProperty;
                    if (!String.IsNullOrEmpty(nav.ReferentialConstraint))
                        values.Add("field", $"'{nav.ReferentialConstraint}'");
                    if (!String.IsNullOrEmpty(nav.ReferencedProperty))
                        values.Add("ref", $"'{nav.ReferencedProperty}'");
                }
                var annots = this.Value.Annotations;
                if (annots.Count > 0) {
                    var json = JsonConvert.SerializeObject(annots.Select(annot => annot.ToDictionary()));
                    values.Add("annotations", $"{json}");
                }
                return $"{{{String.Join(", ", values.Select(p => $"{p.Key}: {p.Value}"))}}}";
            }
        } 
    }
    public class Meta : Structured 
    {
        public Meta(StructuredType type) : base(type) { }
        public override string FileName => this.EdmStructuredType.Name.ToLower() + ".meta";
        public override string Name => this.EdmStructuredType.Name + "Meta";
        public string EntityName => this.EdmStructuredType.Name;

        public string EntitySetAnnotations {
            get {
                if (this.Service is ServiceEntity) 
                    return JsonConvert.SerializeObject((this.Service as ServiceEntity).EdmEntitySet.Annotations.Select(annot => annot.ToDictionary()));
                else if (this.Service is ServiceModel)
                    return JsonConvert.SerializeObject((this.Service as ServiceModel).EdmEntitySet.Annotations.Select(annot => annot.ToDictionary()));
                return "";
            }
        }
        public string EntityAnnotations {
            get {
                return JsonConvert.SerializeObject(this.EdmStructuredType.Annotations.Select(annot => annot.ToDictionary()));
            }
        }

        // Imports
        public override IEnumerable<string> ImportTypes => new List<string> { this.EntityType };

        // Exports
        public override IEnumerable<Angular.StructuredProperty> Properties {
            get {
                var props = this.EdmStructuredType.Properties.ToList();
                if (this.EdmStructuredType is EntityType) 
                    props.AddRange((this.EdmStructuredType as EntityType).NavigationProperties);
                var keys = (this.EdmStructuredType is EntityType) ? (this.EdmStructuredType as EntityType).Keys : new List<PropertyRef>();
                return props.Select(prop => {
                    var type = this.Dependencies.FirstOrDefault(dep => dep.Type == prop.Type);
                    return new MetaField(prop, keys); 
                });
            }
        } 

        public override object ToLiquid()
        {
            return new {
                Name = this.Name,
                Type = this.Type,
                EntityType = this.EntityType
            };
        }
    }
}