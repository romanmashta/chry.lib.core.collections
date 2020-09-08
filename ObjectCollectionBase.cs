using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cherry.Lib.Core.App.Discovery;

namespace Cherry.Lib.Core.Collections
{
    public abstract class ObjectCollectionBase<T> : IObjectCollection
    {
        public abstract string ResourcePath { get; }
        public abstract string Icon { get; }
        public abstract string DisplayName { get; }

        async Task<IEnumerable<object>> IObjectCollection.FetchItems() =>
            (await FetchItems()).Cast<object>();
            
        public abstract Task<IEnumerable<T>> FetchItems();
        public Type QueryView() => typeof(TableView.TableView);
    }
}