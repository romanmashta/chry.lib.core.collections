using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cherry.Lib.Core.App.Discovery;
using Cherry.Lib.Core.Data.Models;

namespace Cherry.Lib.Core.Collections
{
    public interface IObjectResource: IResource
    {
        public string Icon { get; set; }
        public string DisplayName { get; set;}
        public string[] Keywords { get; set;}
        public IObjectWithRef TargetObject { get; set; }        
        List<Accessor> Accesors { get; }
    }
    
    public interface ISaveableResource: IObjectResource
    {
        Action SaveResource { get; set; }
    }
}