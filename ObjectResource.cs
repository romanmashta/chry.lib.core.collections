using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Cherry.Lib.Core.App.Discovery;
using Cherry.Lib.Core.Data.Models;
using Cherry.Lib.Core.Meta;
using Microsoft.Extensions.Localization;
using MongoDbGenericRepository.Attributes;

namespace Cherry.Lib.Core.Collections
{
    public class ObjectResource : IObjectResource, ISaveableResource
    {
        public string Icon { get; set; }
        public string DisplayName { get; set; }
        public string[] Keywords { get; set; }
        public bool WithHeader => true;
        public IObjectWithRef TargetObject { get; set; }

        public List<Accessor> Accesors { get; set; } = new List<Accessor>();

        public Type QueryView() => typeof(ObjectView.ObjectView);

        public static ISaveableResource FromObject(IObjectWithRef targetObject, string icon, string displayName, string[] keywords, IStringLocalizer localizer)
        {
            var resource = new ObjectResource();
            var namedEntity = targetObject as INamedEntity;
            resource.DisplayName = namedEntity?.Name ?? targetObject.GetTypeAttribute<TitleAttribute>()?.GetLocalisedName(localizer) ?? displayName;
            resource.TargetObject = targetObject;
            resource.Accesors = BuildAccessors(targetObject, localizer);
            resource.Icon ??= icon;
            resource.Keywords ??= keywords;
            return resource;
        }

        public static List<Accessor> BuildAccessors(object objectWithRef, IStringLocalizer localizer)
        {
            var properties = objectWithRef
                .GetEntityProperties()
                .Where(p => p.GetMemberAttribute<TitleAttribute>() != null);
            return properties.Select(p => CreateAccessor(objectWithRef, p, localizer)).ToList();
        }

        private static Accessor CreateAccessor(object obj, PropertyInfo prop, IStringLocalizer localizer)
        {
            var collectionName = prop.GetMemberAttribute<LookupAttribute>()?.Name;
            var title = prop.GetMemberAttribute<TitleAttribute>();
            var accessor = new Accessor
            {
                Name = prop.Name,
                Title = title?.GetLocalisedName(localizer) ?? prop.Name,
                Length = title?.Length ?? 0,
                Long = prop.GetMemberAttribute<LongAttribute>() != null,
                Multiline = prop.GetMemberAttribute<MultilineAttribute>() != null,
                ReadOnly = title?.ReadOnly == true,
                Icon = prop.GetMemberAttribute<IconAttribute>()?.Name,
                IsCollection = typeof(IList).IsAssignableFrom(prop.PropertyType),
                IsLookup = collectionName != null,
                CollectionName = collectionName,
                IsEnum = prop.PropertyType.IsEnum,
                Getter = (o) => prop.GetValue(obj),
                Setter = (o, raw) =>
                {
                    var value = raw;
                    if(prop.PropertyType.IsEnum){
                        value = System.Enum.Parse(prop.PropertyType, raw.ToString());
                    }
                    prop.SetValue(o, value);
                },                
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

        public Action SaveResource { get; set; }
    }
}