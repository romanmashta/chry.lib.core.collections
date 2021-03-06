using Autofac;
using Cherry.Lib.Core.App.Discovery;
using Cherry.Lib.Core.App.Extension;
using Cherry.Lib.Core.Data;

namespace Cherry.Lib.Core.Collections
{
    public class CollectionsModule: AppModule
    {
        
    }
    
    public class CollectionsModuleInfo : ModuleInfo<CollectionsModule>
    {
        public override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterModule<DataClientModule>();
            builder.RegisterDiscovery<CollectionsDiscoveryService>();
            builder.RegisterResolver<CollectionResolver>();
        }
    }  
}