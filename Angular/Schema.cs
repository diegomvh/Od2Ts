using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using ODataApiGen.Abstracts;

namespace ODataApiGen.Angular
{
    public class Schema : AngularRenderable, DotLiquid.ILiquidizable
    {
        public Models.Schema EdmSchema { get; private set; }
        public string SchemaName { get; private set; }
        public override string FileName => this.SchemaName.ToLower() + ".schema";
        public override string Name => this.SchemaName + "Schema";
        public Angular.Api Api { get; private set; }
        public Angular.ContainerConfig ApiConfig { get; private set; }
        public ICollection<Angular.Enum> Enums { get; } = new List<Angular.Enum>();
        public ICollection<Angular.EnumConfig> EnumConfigs { get; } = new List<Angular.EnumConfig>();
        public ICollection<Angular.Entity> Entities { get; } = new List<Angular.Entity>();
        public ICollection<Angular.Model> Models { get; } = new List<Angular.Model>();
        public ICollection<Angular.Collection> Collections { get; } = new List<Angular.Collection>();
        public ICollection<Angular.EntityConfig> EntityConfigs { get; } = new List<Angular.EntityConfig>();
        public ICollection<Angular.Container> Containers { get; } = new List<Angular.Container>();
        public Schema(Models.Schema schema, string name, bool models)
        {
            this.EdmSchema = schema;
            this.SchemaName = name;
            this.Api = new Angular.Api(name);
            this.AddEnums(schema.EnumTypes);
            this.AddComplexes(schema.ComplexTypes, models);
            this.AddEntities(schema.EntityTypes, models);
            foreach (var container in schema.EntityContainers)
            {
                this.Containers.Add(new Angular.Container(container, models));
            }
        }
        public void AddEnums(IEnumerable<Models.EnumType> enums)
        {
            foreach (var e in enums)
            {
                var enu = new Angular.Enum(e);
                this.Enums.Add(enu);
                var config = new Angular.EnumConfig(enu);
                this.EnumConfigs.Add(config);
            }
        }
        public void AddComplexes(IEnumerable<Models.ComplexType> complexes, bool models)
        {
            foreach (var cmplx in complexes)
            {
                Angular.EntityConfig config;
                var entity = new Angular.Entity(cmplx);
                this.Entities.Add(entity);
                if (models)
                {
                    var model = new Angular.Model(cmplx, entity);
                    this.Models.Add(model);
                    var collection = new Angular.Collection(cmplx, model);
                    this.Collections.Add(collection);
                    config = new Angular.EntityConfig(entity, model, collection);
                } else {
                    config = new Angular.EntityConfig(entity);
                }
                this.EntityConfigs.Add(config);
            }
        }
        public void AddEntities(IEnumerable<Models.EntityType> entities, bool models)
        {
            foreach (var enty in entities)
            {
                Angular.EntityConfig config;
                var entity = new Angular.Entity(enty);
                this.Entities.Add(entity);
                if (models)
                {
                    var model = new Angular.Model(enty, entity);
                    this.Models.Add(model);
                    var collection = new Angular.Collection(enty, model);
                    this.Collections.Add(collection);
                    config = new Angular.EntityConfig(entity, model, collection);
                } else {
                    config = new Angular.EntityConfig(entity);
                }
                this.EntityConfigs.Add(config);
            }
        }
        // Imports
        public override IEnumerable<string> ImportTypes => new List<string> { };
        public override IEnumerable<string> ExportTypes => new string[] { this.Name };
        public override IEnumerable<Import> Imports => GetImportRecords();
        public override string Namespace => this.EdmSchema.Namespace;
        public override string Directory => this.Namespace.Replace('.', Path.DirectorySeparatorChar);
        public void ResolveDependencies()
        {

            // Enums
            foreach (var enumm in Enums)
            {
            }
            // Entities
            foreach (var entity in Entities)
            {
                if (!String.IsNullOrEmpty(entity.EdmStructuredType.BaseType))
                {
                    var baseEntity = this.Entities.FirstOrDefault(e => e.EdmStructuredType.FullName == entity.EdmStructuredType.BaseType);
                    entity.SetBase(baseEntity);
                    entity.Dependencies.Add(baseEntity);
                }
            }
            // Models
            foreach (var model in Models)
            {
                if (!String.IsNullOrEmpty(model.EdmStructuredType.BaseType))
                {
                    var baseModel = this.Models.FirstOrDefault(e => e.EdmStructuredType.FullName == model.EdmStructuredType.BaseType);
                    model.SetBase(baseModel);
                    model.Dependencies.Add(baseModel);
                }
                var collection = this.Collections.FirstOrDefault(m => m.EdmStructuredType.Name == model.EdmStructuredType.Name);
                if (collection != null)
                {
                    model.SetCollection(collection);
                }
            }
            // Collections
            foreach (var collection in Collections)
            {
                if (!String.IsNullOrEmpty(collection.EdmStructuredType.BaseType))
                {
                    var baseCollection = this.Collections.FirstOrDefault(e => e.EdmStructuredType.FullName == collection.EdmStructuredType.BaseType);
                    collection.SetBase(baseCollection);
                    collection.Dependencies.Add(baseCollection);
                }
            }

            foreach (var container in Containers)
            {
                container.ResolveDependencies(this.Enums, this.Entities, this.Models, this.Collections);
            }

            // Resolve Renderable Dependencies
            var renderables = new List<Renderable>();
            renderables.Add(this.Api);
            renderables.AddRange(this.Enums);
            renderables.AddRange(this.EnumConfigs);
            renderables.AddRange(this.Entities);
            renderables.AddRange(this.Models);
            renderables.AddRange(this.Collections);
            renderables.AddRange(this.EntityConfigs);
            foreach (var renderable in renderables)
            {
                var types = renderable.ImportTypes;
                if (renderable is Enum || renderable is EnumConfig || renderable is Structured || renderable is Service)
                {
                    renderable.Dependencies.AddRange(
    this.Enums.Where(e => e != renderable && types.Contains(e.EdmEnumType.FullName)));
                    if (renderable is Structured || renderable is Service)
                    {
                        renderable.Dependencies.AddRange(
        this.Entities.Where(e => e != renderable && types.Contains(e.EdmStructuredType.FullName)));
                        if (!(renderable is EnumConfig))
                        {
                            {
                                renderable.Dependencies.AddRange(
                this.Models.Where(e => e != renderable && types.Contains(e.EdmStructuredType.FullName)));
                                renderable.Dependencies.AddRange(
                this.Collections.Where(e => e != renderable && types.Contains(e.EdmStructuredType.FullName)));
                            }
                        }
                    }
                }
            }
            this.Dependencies.AddRange(this.EnumConfigs);
            this.Dependencies.AddRange(this.EntityConfigs);
            this.Dependencies.AddRange(this.Containers);
        }
        public IEnumerable<string> GetAllDirectories()
        {
            return new string[] {this.Api.Directory}
                .Union(this.Enums.Select(e => e.Directory))
                .Union(this.Entities.Select(m => m.Directory))
                .Union(this.Models.Select(m => m.Directory))
                .Union(this.EntityConfigs.Select(m => m.Directory))
                .Union(this.EnumConfigs.Select(m => m.Directory))
                .Union(this.Collections.Select(m => m.Directory))
                .Union(this.Containers.SelectMany(c => c.GetAllDirectories()));
        }
        public IEnumerable<Renderable> Renderables
        {
            get
            {
                var renderables = new List<Renderable>();
                renderables.Add(this.Api);
                renderables.AddRange(this.Enums);
                renderables.AddRange(this.EnumConfigs);
                renderables.AddRange(this.Entities);
                renderables.AddRange(this.Models);
                renderables.AddRange(this.Collections);
                renderables.AddRange(this.EntityConfigs);
                renderables.AddRange(this.Containers);
                renderables.AddRange(this.Containers.SelectMany(s => s.Renderables));
                return renderables;
            }
        }
        public object ToLiquid()
        {
            return new
            {
                Name = this.Name,
                Namespace = this.Namespace,
                Type = this.Type
            };
        }
    }
}