// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;

namespace Cake.VisualStudio.Helpers
{
    internal static class Extensions
    {
        internal static void ShowMessageBox(this IServiceProvider provider, string message, string title = "Cake Installer")
        {
            VsShellUtilities.ShowMessageBox(
                provider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        internal static void ShowErrorMessage(this IServiceProvider provider, string message, string title = "Error during Cake installation")
        {
            VsShellUtilities.LogError("Cake.VisualStudio", message);
            VsShellUtilities.ShowMessageBox(
                provider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        internal static void ShowStatusBarText(this DTE2 dte, string text)
        {
            if (dte?.StatusBar == null)
            {
                return;
            }

            dte.StatusBar.Text = text;
        }

        internal static bool RequiresOffset(this ITextSnapshotLine line, params string[] protectedIdentifiers)
        {
            var content = line.GetText().TrimEnd();
            return protectedIdentifiers.Any(i => content.EndsWith(i));
        } 
    }
}