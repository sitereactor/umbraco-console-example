﻿using System;
using System.IO;
using System.Linq;
using Umbraco.Core;

namespace UmbConsole
{
    /// <summary>
    /// Extends the UmbracoApplicationBase, which is needed to start the application with out own BootManager.
    /// </summary>
    public class ConsoleApplicationBase : UmbracoApplicationBase
    {
        public string BaseDirectory { get; private set; }
        public string DataDirectory { get; private set; }

        protected override IBootManager GetBootManager()
        {
            var binDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "bin"));
            BaseDirectory = ResolveBasePath(binDirectory);
            DataDirectory = Path.Combine(BaseDirectory, "app_data");
            var appDomainConfigPath = new DirectoryInfo(Path.Combine(BaseDirectory, "config"));

            //Copy config files to AppDomain's base directory
            if (binDirectory.FullName.Equals(BaseDirectory) == false &&
                appDomainConfigPath.Exists == false)
            {
                appDomainConfigPath.Create();
                var baseConfigPath = new DirectoryInfo(Path.Combine(BaseDirectory, "config"));
                var sourceFiles = baseConfigPath.GetFiles("*.config", SearchOption.TopDirectoryOnly);
                foreach (var sourceFile in sourceFiles)
                {
                    sourceFile.CopyTo(sourceFile.FullName.Replace(baseConfigPath.FullName, appDomainConfigPath.FullName), true);
                }
            }

            AppDomain.CurrentDomain.SetData("DataDirectory", DataDirectory);

            return new ConsoleBootManager(this, BaseDirectory);
        }

        public void Start(object sender, EventArgs e)
        {
            base.Application_Start(sender, e);
        }

        private string ResolveBasePath(DirectoryInfo currentFolder)
        {
            var folders = currentFolder.GetDirectories();
            if (folders.Any(x => x.Name.Equals("app_data", StringComparison.OrdinalIgnoreCase)) &&
                folders.Any(x => x.Name.Equals("config", StringComparison.OrdinalIgnoreCase)))
            {
                return currentFolder.FullName;
            }

            if (currentFolder.Parent == null)
                throw new Exception("Base directory containing an 'App_Data' and 'Config' folder was not found."+
                    " These folders are required to run this console application as it relies on the normal umbraco configuration files.");

            return ResolveBasePath(currentFolder.Parent);
        }
    }
}