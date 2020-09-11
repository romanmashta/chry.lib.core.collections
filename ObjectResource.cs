using System;
using Cherry.Lib.Core.App.Discovery;
using Cherry.Lib.Core.Meta;

namespace Cherry.Lib.Core.Collections
{
    public class ObjectResource:IResource
    {
        public string Icon { get; set; }
        public string DisplayName { get; set;}
        public string[] Keywords { get; set;}
        public IObjectWithRef TargetObject { get; set; }
        public Type QueryView() => typeof(ObjectView.ObjectView);

        public static IResource FromObject(IObjectWithRef targetObject)
        {
            var resource = new ObjectResource();
            var namedEntity = targetObject as INamedEntity;
            resource.DisplayName = namedEntity?.Name ?? targetObject.GetTypeAttribute<TitleAttribute>()?.Name;
            resource.TargetObject = targetObject;
            return resource;
        }
    }
}