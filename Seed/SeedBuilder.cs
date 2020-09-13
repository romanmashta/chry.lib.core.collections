using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Cherry.Lib.Core.App.Discovery;
using ImpromptuInterface;

namespace Cherry.Lib.Core.Collections.Seed
{
    public class SeedBuilder<T> where T: class, IObjectWithRef
    {
        readonly List<T> _result = new List<T>();
        
        public SeedBuilder<T> Add(Action<T> init)
        {
            var obj = ObjectBuilder.CreateObjectOf<T>();
            obj.Ref = Guid.NewGuid().ToString();            
            init(obj);
            _result.Add(obj);
            return this;
        }

        public List<T> Seed() => _result;
        public Task<List<T>> SeedAsync() => Task.FromResult(_result);
    }
    
    public static class SeedBuilder
    {
        public static SeedBuilder<T> For<T>() where T : class, IObjectWithRef => new SeedBuilder<T>();
    } 
}

public static class ObjectBuilder
{
    public static object CreateObjectOf(this Type type)
    {
        var obj = new ExpandoObject() as IDictionary<string, object>;
        var properties = type.GetProperties();
        foreach (var propertyInfo in properties)
        {
            var propType = propertyInfo.PropertyType;
            var value = propType.IsValueType ? Activator.CreateInstance(propType) : null;
            obj.Add(propertyInfo.Name, value);
        }
        
        var typed = Impromptu.DynamicActLike(obj, type);        
        
        return typed;
    }

    public static T CreateObjectOf<T>() where T : class => typeof(T).CreateObjectOf() as T;
}