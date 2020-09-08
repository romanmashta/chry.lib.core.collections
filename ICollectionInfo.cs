using System.Collections.Generic;
using System.Threading.Tasks;
using Cherry.Lib.Core.App.Discovery;

namespace Cherry.Lib.Core.Collections
{
    public interface IObjectCollection : IResource
    {
        string ResourcePath { get; }
        string Icon { get; }
        string DisplayName { get; }

        Task<IEnumerable<object>> FetchItems();
    }    
}