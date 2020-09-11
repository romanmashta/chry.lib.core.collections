using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Cherry.Lib.Core.App.Discovery;
using Cherry.Lib.Core.Meta;
using Skclusive.Material.Icon;

namespace Cherry.Lib.Core.Collections
{
    public class Accessor
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public Func<object, object> Getter { get; set; }

        public static Accessor FromExpression<T>(Expression<Func<T, object>> expression)
        {
            var accessor = new Accessor();
            var accessedProperty = expression.GetPropertyInfo();
            accessor.Name = accessedProperty.Name;
            accessor.Title = accessedProperty.GetMemberAttribute<TitleAttribute>()?.Name ?? accessedProperty.Name;
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
        
        public string[] Keywords { get; set; }

        public List<Accessor> Accesors { get; }

        protected ObjectCollectionBase(string resourcePath, 
            string displayName, 
            string icon,
            Accessors<T> accessors,
            string[] keywords = null
            )
        {
            DisplayName = displayName;
            Icon = icon;
            ResourcePath = resourcePath;
            Accesors = BuildAccessors(accessors);
            Keywords = keywords;
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
        private List<T> _collection = null;
        public InmemoryCollection(string resourcePath, string displayName, string icon, Accessors<T> accessors, string[] keywords = null) : base(resourcePath, displayName, icon, accessors, keywords)
        {
        }

        public override async Task<IEnumerable<T>> FetchItems() => await (_collection == null
            ? Task.FromResult(_collection = await LoadItems())
            : Task.FromResult(_collection));

        protected abstract Task<List<T>> LoadItems();

        public override async Task<IResource> ResolveResource(string objectRef)
        {
            var items = await FetchItems();
            var targetObject = items.Cast<IObjectWithRef>().FirstOrDefault(o => o.Ref == objectRef);
            var resorce = ObjectResource.FromObject(targetObject);
            resorce.Icon ??= this.Icon;
            resorce.DisplayName ??= this.DisplayName;
            resorce.Keywords ??= this.Keywords;
            return await Task.FromResult(resorce);
        }
    }

    public class Accessors<T> : List<Expression<Func<T, object>>>
    {
        
    }
}