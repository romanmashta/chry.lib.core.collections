using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cherry.Lib.Core.App.Discovery;

namespace Cherry.Lib.Core.Collections
{
    public class CollectionResolver: IResourceResolver
    {
        private readonly IEnumerable<IObjectCollection> _collections;

        public CollectionResolver(IEnumerable<IObjectCollection> collections)
        {
            _collections = collections;
        }

        public Task<IResource> ResolveResource(string resourcePath) =>
            Task.FromResult(_collections.FirstOrDefault(c => c.ResourcePath == resourcePath) as IResource);
    }
}