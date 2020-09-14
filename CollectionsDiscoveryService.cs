using System.Collections.Generic;
using System.Linq;
using Cherry.Lib.Core.App.Discovery;

namespace Cherry.Lib.Core.Collections
{
    public class CollectionsDiscoveryService : IDiscoveryService
    {
        private readonly IEnumerable<IObjectCollection> _collections;
        public IEnumerable<IDiscoveryItem> Items { get; }

        public CollectionsDiscoveryService(IEnumerable<IObjectCollection> collections)
        {
            _collections = collections;
            Items = BuildDiscovery();
        }

        private IEnumerable<DiscoveryItem> BuildDiscovery() =>
            _collections.Select(c => new DiscoveryItem
            {
                ResourcePath = c.ResourcePath,
                MetaInfo = new MetaInfo
                {
                    Priority = c.IntentionPriority,
                    Icon = c.Icon,
                    DisplayName = c.DisplayName,
                    Badge = c.Badge
                }
            });
    }
}