using System.Collections.Generic;
using System.Threading.Tasks;
using Cherry.Lib.Core.App.Discovery;

namespace Cherry.Lib.Core.Collections
{
    public interface IObjectCollection : IResource, ISortable, IResourceResolver
    {
        string ResourcePath { get; }
        string Icon { get; }
        string DisplayName { get; }       
        List<Accessor> Accesors { get; }
        Task<IEnumerable<object>> FetchItems();
    }
}