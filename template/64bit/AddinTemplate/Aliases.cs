using System;
using System.Collections.Generic;
$if$ ($targetframeworkversion$ >= 3.5)using System.Linq;
$endif$using System.Text;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;

namespace $safeprojectname$
{
	[CakeAliasCategory("Sample")]
	public static class AddinAliases
	{
		[CakeMethodAlias]
		public static void Hello(this ICakeContext ctx, string name)
		{
			ctx.Log.Information("Hello " + name);
		}
	}
}
