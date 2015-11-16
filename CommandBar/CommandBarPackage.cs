//------------------------------------------------------------------------------
// <copyright file="CommandBarPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using EnvDTE80;
using EnvDTE;

namespace CommandBar
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(CommandBarPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(UIContextGuids80.CodeWindow, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class CommandBarPackage : Package
    {
        public const string PackageGuidString = "de9425e1-8d58-4756-aa02-8248b575a1bd";

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBarPackage"/> class.
        /// </summary>
        public CommandBarPackage()
        {
        }

        #region Package Members

        protected override void Initialize()
        {
            base.Initialize();
            CommandBarOpenCommand.Initialize(this);

        }

        #endregion
    }
}
