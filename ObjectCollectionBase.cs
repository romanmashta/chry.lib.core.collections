using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cherry.Lib.Core.App.Discovery;
using Cherry.Lib.Core.Data.Clients.Contracts;
using Cherry.Lib.Core.Data.Models;
using Cherry.Lib.Core.Meta;
using Microsoft.AspNetCore.Components;
using Serilog;
using Skclusive.Material.Icon;

namespace Cherry.Lib.Core.Collections
{
    public class Accessor
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public Func<object, object> Getter { get; set; }
        public bool Long { get; set; }
        public string Icon { get; set; }
        public bool Multiline { get; set; }
        
        public bool IsCollection { get; set; }
        
        public Type AccessorType { get; set; }

        public int Length { get; set; } = 0;

        public static Accessor FromExpression<T>(Expression<Func<T, object>> expression)
        {
            var accessor = new Accessor();
            var accessedProperty = expression.GetPropertyInfo();
            var attr = accessedProperty.GetMemberAttribute<TitleAttribute>();
            accessor.Name = accessedProperty.Name;
            accessor.Title = attr?.Name ?? accessedProperty.Name;
            accessor.Length = attr?.Length ?? 0;
            accessor.AccessorType = accessedProperty.PropertyType;
            var getter = expression.Compile();
            accessor.Getter = (o) => getter((T) o);

            return accessor;        
        }
    }
    public abstract class ObjectCollectionBase<T> : IObjectCollection
    {
        public Accessor SortedBy { get; set; }
        public SortDirection Direction { get; set; }        
        public string ResourcePath { get; }
        public string Icon { get; set; }
        public string DisplayName { get; set; }
        public virtual Priority IntentionPriority => Priority.Middle;
        public int? Order { get; set; }
        public virtual bool CanAdd => true;
        public int? Badge => GetBadgeCount();

        protected virtual int? GetBadgeCount() => null;

        public string[] Keywords { get; set; }
        public bool WithHeader => true;

        public List<Accessor> Accesors { get; }

        protected ObjectCollectionBase(string resourcePath, 
            string displayName, 
            string icon,
            Accessors<T> accessors,
            string[] keywords = null,
            int? order = null
            )
        {
            DisplayName = displayName;
            Icon = icon;
            ResourcePath = resourcePath;
            Accesors = BuildAccessors(accessors);
            Keywords = keywords;
            Order = order;
        }

        private List<Accessor> BuildAccessors(Accessors<T> accessors)  
            => accessors.Select(e =>Accessor.FromExpression<T>(e)).ToList();

        async Task<IEnumerable<object>> IObjectCollection.FetchItems() {
            var items = (await FetchItems()).Cast<object>();
            if(SortedBy !=null && Direction != SortDirection.None)
            {
                items = Direction == SortDirection.Asc
                    ? items.OrderBy(SortedBy.Getter)
                    : items.OrderByDescending(SortedBy.Getter);
            }
            return items;
        }

        public abstract Task<IEnumerable<T>> FetchItems();
        
        
        public Type QueryView() => typeof(TableView.TableView);
        
        public abstract Task<IResource> ResolveResource(string objectRef);
    }

    public abstract class InmemoryCollection<T> : ObjectCollectionBase<T>
    {
        protected List<T> _collection = null;

        public InmemoryCollection(string resourcePath, string displayName, string icon, Accessors<T> accessors, string[] keywords = null, int? order = null) : base(resourcePath, displayName, icon, accessors, keywords, order)
        {
        }

        public override async Task<IEnumerable<T>> FetchItems() => await (_collection == null
            ? Task.FromResult(_collection = await LoadItems())
            : Task.FromResult(_collection));

        protected virtual async Task<List<T>> LoadItems()
        {
            return new List<T>();
        }

        public override async Task<IResource> ResolveResource(string objectRef)
        {
            var items = await FetchItems();
            IObjectWithRef targetObject = null;
            if (objectRef == Objects.NewObjectUri)
            {
                targetObject = (IObjectWithRef) typeof(T).CreateObjectOf();
                targetObject.Ref = Guid.NewGuid().ToString();
            }
            else
            {
                targetObject = items.Cast<IObjectWithRef>().FirstOrDefault(o => o.Ref == objectRef);
            }
            
            var resorce = ObjectResource.FromObject(targetObject, Icon, DisplayName, Keywords);
            return await Task.FromResult(resorce);
        }
    }

    public class EntityCollection<T> : InmemoryCollection<T>
    {
        private readonly IRepositoryClient<T> _repositoryClient;

        public EntityCollection(
            IRepositoryClient<T> repositoryClient,
            string resourcePath,
            string displayName,
            string icon,
            Accessors<T> accessors,
            string[] keywords = null,
            int? order = null)
            : base(resourcePath, displayName, icon, accessors, keywords, order)
        {
            _repositoryClient = repositoryClient;
        }

        public override async Task<IEnumerable<T>> FetchItems()
        {
            var entities = await _repositoryClient.GetEntities(ResourcePath);
            return entities.Items.ToList();
        }
    }

    public class Accessors<T> : List<Expression<Func<T, object>>>
    {
        
    }
}