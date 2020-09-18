using System.Collections.Generic;
using System.Threading.Tasks;
using Cherry.Lib.Core.App.Discovery;

namespace Cherry.Lib.Core.Collections
{
    public interface IObjectCollection : IResource, ISortable, IResourceResolver
    {
        bool CanAdd { get; }
        string ResourcePath { get; }
        string Icon { get; }
        string DisplayName { get; }
        Priority IntentionPriority { get; }
        int? Order { get; set; }
        
        int? Badge { get; }
        List<Accessor> Accesors { get; }
        Task<IEnumerable<object>> FetchItems();
    }
}