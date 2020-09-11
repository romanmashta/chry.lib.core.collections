using System;
using System.Collections.Generic;
using System.Dynamic;
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
            var obj = new ExpandoObject();
            T typed = obj.ActLike<T>();
            typed.Ref = Guid.NewGuid().ToString();            
            init(typed);
            _result.Add(typed);
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