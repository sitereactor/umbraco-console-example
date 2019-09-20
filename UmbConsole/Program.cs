using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Security.Policy;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace UmbConsole
{
    /// <summary>
    /// Before running this console app please ensure that the "umbracoDbDSN" ConnectionString is pointing to your database.
    /// If you are using Sql Ce please replace "|DataDirectory|" with a real path or alternatively place 
    /// your database in the debug folder before running the application in debug mode.
    /// </summary>
    class Program
    {
        private static string toolPath = AppDomain.CurrentDomain.BaseDirectory;
        private static Dictionary<string, Assembly> environmentAssemblies = null;
        private static ConsoleApplicationBase Application;

        static void Main(string[] args)
        {
            Console.WriteLine("Hanging to debug");
            Console.ReadKey(true);

            CreateAndRunDomain(args);
        }

        private static void CreateAndRunDomain(string[] args)
        {
            var umbracoDomain = AppDomain.CreateDomain(
                "Umbraco",
                new Evidence(),
                new AppDomainSetup
                {
                    //ApplicationBase = Environment.CurrentDirectory,
                    //PrivateBinPath = Path.Combine(Environment.CurrentDirectory, "bin"),
                    //PrivateBinPathProbe = "NonNullToOnlyUsePrivateBin",
                    ConfigurationFile = Path.Combine(Environment.CurrentDirectory, "web.config")
                }
            );
            umbracoDomain.SetData("args", args);
            umbracoDomain.SetData(".appPath", Environment.CurrentDirectory);

            //var assembly = File.ReadAllBytes(Path.Combine(toolPath, "UmbConsole.exe"));
            //umbracoDomain.Load(assembly);

            //umbracoDomain.AssemblyLoad += (sender, eventArgs) => { return; };
            umbracoDomain.AssemblyResolve += AssemblyResolve;

            try
            {
                umbracoDomain.DoCallBack(RunUmbraco);
            }
            catch(Exception ex)
            {
                throw new Exception("Could not boot Umbraco. Likely not an Umbraco folder, or assemblies need to be eagerly loaded. Pass assemblies as arguments.", ex);
            }
        }

        private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var assemblyName = args.Name;

                var probeFolder = Path.Combine(Environment.CurrentDirectory, "bin");

                var currentDomain = AppDomain.CurrentDomain;
                if (environmentAssemblies == null)
                {
                    var allLoadedAssemblies = currentDomain.GetAssemblies().Where(x => !x.IsDynamic);
                    var loadedAssemblyFileNames = allLoadedAssemblies
                        .Select(x => {
                            try
                            {
                                return x.Location.Substring(x.Location.LastIndexOf(@"\") + 1);
                            }
                            catch
                            {
                                throw;
                            }
                        })
                        .ToArray();

                    var allEnvironmentAssemblyFiles = Directory.GetFiles(probeFolder)
                        .Where(x => x.EndsWith(".dll") || x.EndsWith(".exe"))
                        .ToArray();

                    var notLoadedEnvironmentAssemblies = allEnvironmentAssemblyFiles
                        .Where(x => !loadedAssemblyFileNames.Any(x.EndsWith))
                        .ToArray();

                    environmentAssemblies = notLoadedEnvironmentAssemblies
                        .Select(Assembly.LoadFrom)
                        .ToDictionary(x => x.GetName().FullName, x => x);
                }

                var isMatchedFullName = environmentAssemblies.ContainsKey(assemblyName);
                if (!isMatchedFullName)
                {
                    var fromSimpleName =
                        environmentAssemblies.Keys.FirstOrDefault(x => x.StartsWith(assemblyName + ","));
                    if (!String.IsNullOrEmpty(fromSimpleName))
                    {
                        assemblyName = fromSimpleName;
                        isMatchedFullName = true;
                    }
                }

                if (isMatchedFullName)
                {
                    return environmentAssemblies[assemblyName];
                }

                Debug.WriteLine("Couldn't find " + assemblyName);

                return null;
            }
            catch
            {
                throw;
            }
        }

        private static void RunUmbraco()
        {
            Console.Title = "Umbraco Console";

            var assembliesToLoad = (string[])AppDomain.CurrentDomain.GetData("args");
            foreach (var assembly in assembliesToLoad)
            {
                Assembly.Load(assembly);
            }
            //Assembly.Load("Examine");
            //Assembly.Load("Lucene.Net");
            //Assembly.Load("Umbraco.Forms.Web");

            try
            {
                //Initialize the application
                var context = InitializeApplication();

                //if (ExecuteTypeIfSpecified(context))
                //{
                //    return;
                //}

                PSLoop();

                //MainLoop(context);
            }
            catch
            {
                // here to debug within domain
                throw;
            }
        }

        private static void Output<T>(object collection, DataAddedEventArgs args)
        {
            var dataCollection = (PSDataCollection<T>) collection;
            var value = dataCollection.Last();
            Console.WriteLine(value);
        }

        private static void PSLoop()
        {
            var listener = new PSListenerConsoleSample();
            listener.Run();
        }

        private static void OldPSLoop()
        {

            using (Runspace rs = RunspaceFactory.CreateRunspace())
            {
                rs.Open();
                
                rs.SessionStateProxy.SetVariable("applicationContext", ApplicationContext.Current);
                rs.SessionStateProxy.SetVariable("greeting", "Hi there from C#!");

                using (var shell = PowerShell.Create())
                {
                    shell.Streams.Debug.DataAdded += Output<DebugRecord>;
                    shell.Streams.Error.DataAdded += Output<ErrorRecord>;
                    shell.Streams.Information.DataAdded += Output<InformationRecord>;
                    shell.Streams.Progress.DataAdded += Output<ProgressRecord>;
                    shell.Streams.Verbose.DataAdded += Output<VerboseRecord>;
                    shell.Streams.Warning.DataAdded += Output<WarningRecord>;

                    //var variable = new PSVariable("applicationContext", ApplicationContext.Current);
                    //var variable2 = new PSVariable("greeting", "Hi there!");
                    //var variable3 = new PSVariable("$greeting", "Hi there $!");

                    //shell.AddParameter("$applicationContext", ApplicationContext.Current);
                    while (true)
                    {
                        Console.Write("> ");
                        var input = Console.ReadLine();

                        if (input == "q" || input == "quit")
                        {
                            break;
                        }

                        shell.AddScript(input);
                        var result = shell.Invoke();
                    }
                }

                rs.Close();
            }
        }

        //private static bool shouldExit = false;

        //public class CanIBeHost : PSHost
        //{
        //    public override void SetShouldExit(int exitCode)
        //    {
        //        shouldExit = true;
        //    }

        //    public override void EnterNestedPrompt()
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void ExitNestedPrompt()
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void NotifyBeginApplication()
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override void NotifyEndApplication()
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public override string Name => "Whoah" 
        //    public override Version Version => new Version(1, 0);
        //    public override Guid InstanceId => new Guid("9A0D85D7-914C-4EE6-AC78-9FA493577E22");
        //    public override PSHostUserInterface UI => null;
        //    public override CultureInfo CurrentCulture { get; }
        //    public override CultureInfo CurrentUICulture { get; }
        //}

        private static void MainLoop(ApplicationContext context)
        {
//Exit the application?
            var waitOrBreak = true;
            while (waitOrBreak)
            {
                //List options
                Console.WriteLine("-- Options --");
                Console.WriteLine("List content nodes: l");
                Console.WriteLine("Create new content: c");
                Console.WriteLine("Create Umbraco database schema in empty db: d");
                Console.WriteLine("Execute type :e");
                Console.WriteLine("Quit application: q");

                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("q"))
                    waitOrBreak = false; //Quit the application
                else if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("l"))
                    ListContentNodes(); //Call the method that lists all the content nodes
                else if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("c"))
                    CreateNewContent();
                //Call the method that does the actual creation and saving of the Content object
                else if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("d"))
                    CreateDatabaseSchema();
                else if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("e"))
                    ExecuteType(context);
            }
        }

        private static bool ExecuteTypeIfSpecified(ApplicationContext context)
        {
            var args = AppDomain.CurrentDomain.GetData("args") as string[];
            var executeTypeName = "";
            if (args != null && args.Length > 0)
                executeTypeName = args[0];
            if (!String.IsNullOrWhiteSpace(executeTypeName))
            {
                ExecuteType(context, executeTypeName);
                return true;
            }

            return false;
        }

        private static ApplicationContext InitializeApplication()
        {
            Application = new ConsoleApplicationBase();
            Application.Start(Application, new EventArgs());
            Console.WriteLine("Application Started");

            var context = ApplicationContext.Current;
            var databaseContext = context.DatabaseContext;
            var database = databaseContext.Database;

            Console.WriteLine("--------------------");
            //Write status for ApplicationContext
            Console.WriteLine("ApplicationContext is available: " + (context != null).ToString());
            //Write status for DatabaseContext
            Console.WriteLine("DatabaseContext is available: " + (databaseContext != null).ToString());
            //Write status for Database object
            Console.WriteLine("Database is available: " + (database != null).ToString());
            Console.WriteLine("--------------------");
            return context;
        }

        private static void ExecuteType(ApplicationContext context, string typeName = null)
        {
            if (String.IsNullOrWhiteSpace(typeName))
            {
                Console.WriteLine("Enter typename:");
                typeName = Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Executing " + typeName);
            }
            var type = Type.GetType(typeName ?? "");
            if (type == null)
            {
                Console.WriteLine("Couldn't instantiate type '{0}'", typeName);
                return;
            }

            var ctor = type.GetConstructor(new Type[0]);
            if (ctor == null)
            {
                Console.WriteLine("Couldn't find public parameterless constructor for type '{0}'", typeName);
                return;
            }

            var executeMethod = type.GetMethod("Execute", new[] {typeof(ApplicationContext)});
            if (executeMethod == null)
            {
                Console.WriteLine("The type '{0}' does not implement method 'Execute(ApplicationContext)'", typeName);
                return;
            }

            try
            {
                var instance = ctor.Invoke(new object[0]);
                executeMethod.Invoke(instance, new object[] {context});
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to execute:");
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Private method to list all content nodes
        /// </summary>
        /// <param name="contentService"></param>
        private static void ListContentNodes()
        {
            var contentService = ApplicationContext.Current.Services.ContentService;

            //Get the Root Content
            var rootContent = contentService.GetRootContent();
            foreach (var content in rootContent)
            {
                Console.WriteLine("Root Content: " + content.Name + ", Id: " + content.Id);
                //Get Descendants of the current content and write it to the console ordered by level
                var descendants = contentService.GetDescendants(content);
                foreach (var descendant in descendants.OrderBy(x => x.Level))
                {
                    Console.WriteLine("Name: " + descendant.Name + ", Id: " + descendant.Id + " - Parent Id: " + descendant.ParentId);
                }
            }
        }

        /// <summary>
        /// Private method to create new content
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="contentTypeService"></param>
        private static void CreateNewContent()
        {
            var contentService = ApplicationContext.Current.Services.ContentService;
            var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;


            //We find all ContentTypes so we can show a nice list of everything that is available
            var contentTypes = contentTypeService.GetAllContentTypes();
            var contentTypeAliases = string.Join(", ", contentTypes.Select(x => x.Alias));

            Console.WriteLine("Please enter the Alias of the ContentType ({0}):", contentTypeAliases);
            var contentTypeAlias = Console.ReadLine();

            Console.WriteLine("Please enter the Id of the Parent:");
            var strParentId = Console.ReadLine();
            int parentId;
            if (int.TryParse(strParentId, out parentId) == false)
                parentId = -1;//Default to -1 which is the root

            Console.WriteLine("Please enter the name of the Content to create:");
            var name = Console.ReadLine();

            //Create the Content
            var content = contentService.CreateContent(name, parentId, contentTypeAlias);
            foreach (var property in content.Properties)
            {
                Console.WriteLine("Please enter the value for the Property with Alias '{0}':", property.Alias);
                var value = Console.ReadLine();
                var isValid = property.IsValid(value);
                if (isValid)
                {
                    property.Value = value;
                }
                else
                {
                    Console.WriteLine("The entered value was not valid and thus not saved");
                }
            }

            //Save the Content
            contentService.Save(content);

            Console.WriteLine("Content was saved: " + content.HasIdentity);
        }

        /// <summary>
        /// Private method to install the umbraco database schema in an empty database
        /// </summary>
        /// <param name="database"></param>
        /// <param name="databaseProvider"></param>
        /// <param name="dataDirectory"></param>
        private static void CreateDatabaseSchema()
        {
            Console.WriteLine("Please note that installing the umbraco database schema requires an empty database configured in config.");
            Console.WriteLine("The 'umbracoConfigurationStatus' under appSettings should be left blank.");
            Console.WriteLine("If you are using Sql Ce an empty Umbraco.sdf file should exist in the DataDictionary.");
            Console.WriteLine("Press y to continue");

            var context = ApplicationContext.Current;
            var databaseContext = context.DatabaseContext;
            var database = databaseContext.Database;
            var databaseProvider = database.GetDatabaseProvider();
            var dataDirectory = Application.DataDirectory;

            var input = Console.ReadLine();
            if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("y"))
            {
                try
                {
                    if (databaseProvider == DatabaseProviders.SqlServerCE)
                    {
                        var dbPath = Path.Combine(dataDirectory, "Umbraco.sdf");
                        if (File.Exists(dbPath) == false)
                        {
                            var engine = new SqlCeEngine(@"Data Source=|DataDirectory|\Umbraco.sdf;Flush Interval=1;");
                            engine.CreateDatabase();
                        }
                    }

                    database.CreateDatabaseSchema(false);

                    Console.WriteLine("The database schema has been installed");
                    Console.WriteLine("Note: This is just an example, so no backoffice user has been created.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occured while trying to install the database schema");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
    }
}
