using System;
using Umbraco.Core;

namespace UmbConsole
{
    /// <summary>
    /// Extends the UmbracoApplicationBase, which is needed to start the application with out own BootManager.
    /// </summary>
    public class ConsoleApplicationBase : UmbracoApplicationBase
    {
        protected override IBootManager GetBootManager()
        {
            return new ConsoleBootManager(this);
        }

        public void Start(object sender, EventArgs e)
        {
            base.Application_Start(sender, e);
        }
    }
}