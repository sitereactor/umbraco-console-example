using System.Management.Automation;

namespace Our.Umbraco.Community.PowerShellModule
{
    [Cmdlet("Get", "Greeting")]
    public class GetGreeting : PSCmdlet
    {
        static int greets = 0;

        [Parameter(Mandatory = false, Position = 0, ValueFromPipeline = true)] public string Greetee { get; set; } = "Stranger";

        protected override void ProcessRecord()
        {
            WriteObject($"Hi there, {Greetee} #{++greets}!");
        }
    }
}
