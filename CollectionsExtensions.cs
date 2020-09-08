using Autofac;
using Cherry.Lib.Core.App.Extension;

namespace Cherry.Lib.Core.Collections
{
    public static class CollectionsExtensions
    {
        public static void RegisterCollectionsInModule<T>(this ContainerBuilder builder) where T : AppModule
        {
            builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .PublicOnly()
                .Where(t => typeof(IObjectCollection).IsAssignableFrom(t) && t.IsAbstract == false)
                .As<IObjectCollection>();
        }
    }
}