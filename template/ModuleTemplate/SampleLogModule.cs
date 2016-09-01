using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;

[assembly: CakeModule(typeof($safeprojectname$.SampleLogModule))]

namespace $safeprojectname$
{
    public class SampleLogModule : ICakeModule
{
    public void Register(ICakeContainerRegistry registry)
    {
        registry.RegisterType<ReverseLog>().As<ICakeLog>().Singleton();
    }
}
}
