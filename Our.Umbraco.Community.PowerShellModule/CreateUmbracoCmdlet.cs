using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.Community.PowerShellModule
{
    class UmbracoInstanceContainer
    {
        internal static UmbracoInstance instance = null;

        internal static UmbracoInstance Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UmbracoInstance();
                    instance.Start();
                }

                return instance;
            }
        }
    }

    [Cmdlet("Create", "UmbracoInstance")]
    public class CreateUmbracoCmdlet : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject(UmbracoInstanceContainer.Instance);
        }
    }
}
