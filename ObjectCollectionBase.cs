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
using Microsoft.Extensions.Localization;
using Serilog;
using Skclusive.Material.Icon;

namespace Cherry.Lib.Core.Collections
{
    public class Accessor
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public Func<object, object> Getter { get; set; }
        public Action<object, object> Setter { get; set; }
        public bool Long { get; set; }
        public string Icon { get; set; }
        public bool Multiline { get; set; }
        
        public bool IsCollection { get; set; }
        
        public Type AccessorType { get; set; }

        public int Length { get; set; } = 0;
        
        public bool IsEnum { get; set; }

        public static Accessor FromExpression<T>(Expression<Func<T, object>> expression, IStringLocalizer localizer)
        {
            var accessor = new Accessor();
            var accessedProperty = expression.GetPropertyInfo();
            var attr = accessedProperty.GetMemberAttribute<TitleAttribute>();
            accessor.Name = accessedProperty.Name;
            accessor.Title = attr?.GetLocalisedName(localizer) ?? accessedProperty.Name;
            accessor.Length = attr?.Length ?? 0;
            accessor.AccessorType = accessedProperty.PropertyType;
            var getter = expression.Compile();
            accessor.Getter = (o) => getter((T) o);
            return accessor;        
        }
    }
    public abstract class ObjectCollectionBase<T> : IObjectCollection
    {
        private readonly IStringLocalizer _stringLocalizer;
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

        protected ObjectCollectionBase(
            string resourcePath, 
            string displayName, 
            string icon,
            Accessors<T> accessors,
            string[] keywords = null,
            int? order = null,
            IStringLocalizer stringLocalizer = null         
            )
        {
            _stringLocalizer = stringLocalizer;
            DisplayName = displayName;
            Icon = icon;
            ResourcePath = resourcePath;
            Accesors = BuildAccessors(accessors);
            Keywords = keywords;
            Order = order;
        }

        private List<Accessor> BuildAccessors(Accessors<T> accessors)  
            => accessors.Select(e =>Accessor.FromExpression<T>(e, _stringLocalizer)).ToList();

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

        public abstract Task<CollectionResult> FetchItemsWithSummary();

        public abstract Task<IEnumerable<T>> FetchItems();
        
        
        public Type QueryView() => typeof(TableView.TableView);
        
        public abstract Task<IResource> ResolveResource(string objectRef);
    }

    public abstract class InmemoryCollection<T> : ObjectCollectionBase<T>
    {
        private readonly IStringLocalizer _stringLocalizer;
        protected List<T> _collection = null;

        public InmemoryCollection(string resourcePath, string displayName, string icon, Accessors<T> accessors, string[] keywords = null, int? order = null, IStringLocalizer stringLocalizer = null) : 
            base(resourcePath, displayName, icon, accessors, keywords, order, stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
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
            
            var resorce = ObjectResource.FromObject(targetObject, Icon, DisplayName, Keywords, _stringLocalizer);
            resorce.SaveResource = () => SaveObject(targetObject);
            return await Task.FromResult(resorce);
        }

        public override async Task<CollectionResult> FetchItemsWithSummary()
        {
            return new CollectionResult<T>()
            {
                Items = FetchItems().Result
            };
        }

        public virtual async Task SaveObject(IObjectWithRef targetObject)
        {
            
        }
    }

    public class EntityCollection<T> : InmemoryCollection<T>
    {
        private readonly IRepositoryClient<T> _repositoryClient;
        private string _collectionName;

        public EntityCollection(
            IRepositoryClient<T> repositoryClient,
            string resourcePath,
            string displayName,
            string icon,
            Accessors<T> accessors,
            string[] keywords = null,
            int? order = null, 
            IStringLocalizer stringLocalizer = null,
            string collectionName = null
            )
            : base(resourcePath, displayName, icon, accessors, keywords, order, stringLocalizer)
        {
            _repositoryClient = repositoryClient;
            _collectionName = collectionName;
        }

        public override async Task<IEnumerable<T>> FetchItems()
        {
            var entities = await _repositoryClient.GetEntities(_collectionName ?? ResourcePath, ResourcePath);
            return entities.Items.ToList();
        }

        public override async Task<CollectionResult> FetchItemsWithSummary()
        {
            var result = await _repositoryClient.GetEntities(_collectionName ?? ResourcePath, ResourcePath);
            return result;
        }

        public override async Task SaveObject(IObjectWithRef targetObject)
        {
            await _repositoryClient.Update(ResourcePath, targetObject.Ref, targetObject);
        }
    }

    public class Accessors<T> : List<Expression<Func<T, object>>>
    {
        
    }
}