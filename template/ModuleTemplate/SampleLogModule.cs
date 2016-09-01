using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Composition;
using Cake.Core.Diagnostics;
using System;
using System.Collections.Generic;
$if$ ($targetframeworkversion$ >= 3.5)using System.Linq;
$endif$using System.Text;

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
