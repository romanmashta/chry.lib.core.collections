using System;
using System.Threading.Tasks;
using Cherry.Lib.Core.Disco.ContentViewer;
using Cherry.Lib.Core.Disco.Navigation;
using Microsoft.AspNetCore.Components;

namespace Cherry.Lib.Core.Collections.ObjectView
{
    public class ObjectViewComponent: ResourceViewBase<ObjectResource>
    {
        [Inject] 
        public INavigator Navigator { get; set; }
        
        protected void SaveActionClick(EventArgs obj)
        {
            Resource.SaveResource?.Invoke();
            Navigator.PopResource();
        }        
    }
}