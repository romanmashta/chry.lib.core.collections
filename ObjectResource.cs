using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Cherry.Lib.Core.App.Discovery;
using Cherry.Lib.Core.Meta;

namespace Cherry.Lib.Core.Collections
{
    public class ObjectResource : IObjectResource
    {
        public string Icon { get; set; }
        public string DisplayName { get; set; }
        public string[] Keywords { get; set; }
        public IObjectWithRef TargetObject { get; set; }

        public List<Accessor> Accesors { get; set; } = new List<Accessor>();

        public Type QueryView() => typeof(ObjectView.ObjectView);

        public static IResource FromObject(IObjectWithRef targetObject)
        {
            var resource = new ObjectResource();
            var namedEntity = targetObject as INamedEntity;
            resource.DisplayName = namedEntity?.Name ?? targetObject.GetTypeAttribute<TitleAttribute>()?.Name;
            resource.TargetObject = targetObject;
            resource.Accesors = BuildAccessors(targetObject);
            return resource;
        }

        private static List<Accessor> BuildAccessors(IObjectWithRef objectWithRef)
        {
            var properties = objectWithRef
                .GetEntityProperties()
                .Where(p => p.GetMemberAttribute<TitleAttribute>() != null);
            return properties.Select(p => CreateAccessor(objectWithRef, p)).ToList();
        }

        private static Accessor CreateAccessor(IObjectWithRef obj, PropertyInfo prop)
        {
            var accessor = new Accessor
            {
                Name = prop.Name,
                Title = prop.GetMemberAttribute<TitleAttribute>()?.Name ?? prop.Name,
                Long = prop.GetMemberAttribute<LongAttribute>() != null,
                Multiline = prop.GetMemberAttribute<MultilineAttribute>() != null,
                Icon = prop.GetMemberAttribute<IconAttribute>()?.Name,
                Getter = (o) => prop.GetValue(obj),
                AccessorType = prop.PropertyType
            };
            return accessor;
        }

        private static void BindAccessor(Accessor ac
            , IObjectWithRef obj, PropertyInfo prop)
        {
            if(prop.PropertyType.IsAssignableTo<string>())
                BindString(ac, obj, prop);
            if(prop.PropertyType.IsAssignableTo<int>())
                BindInt(ac, obj, prop);
            BindString(ac, obj, prop);
        }
        
        private static void BindInt(Accessor ac
            , IObjectWithRef obj, PropertyInfo prop)
        {
            ac.Getter = (o) => prop.GetValue(obj);
        }        
        
        private static void BindString(Accessor ac
            , IObjectWithRef obj, PropertyInfo prop)
        {
            ac.Getter = (o) => prop.GetValue(obj);
        }
    }
}