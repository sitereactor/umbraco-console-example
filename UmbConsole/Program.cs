using System;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

namespace UmbConsole
{
    /// <summary>
    /// + + + + + + + + + + + + + + + + + + + +
    /// +   Waahoooo!!! HAPPY BIRTHDAY!! :-)  +
    /// + + + + + + + + + + + + + + + + + + + +
    /// Before running this console app please ensure that the "umbracoDbDSN" ConnectionString is pointing to your database.
    /// If you are using Sql Ce please replace "|DataDirectory|" with a real path or alternatively place 
    /// your database in the debug folder before running the application in debug mode.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Umbraco Console";

            //Initialize the application
            var application = new ConsoleApplicationBase();
            application.Start(application, new EventArgs());
            Console.WriteLine("Application Started");
            
            Console.WriteLine("--------------------");
            //Write status for ApplicationContext
            var context = ApplicationContext.Current;
            Console.WriteLine("ApplicationContext is available: " + (context != null).ToString());
            //Write status for DatabaseContext
            var databaseContext = context.DatabaseContext;
            Console.WriteLine("DatabaseContext is available: " + (databaseContext != null).ToString());
            //Write status for Database object
            var database = databaseContext.Database;
            Console.WriteLine("Database is available: " + (database != null).ToString());
            Console.WriteLine("--------------------");

            //Get the ServiceContext and the two services we are going to use
            var serviceContext = context.Services;
            var contentService = serviceContext.ContentService;
            var contentTypeService = serviceContext.ContentTypeService;

            //Exit the application?
            var waitOrBreak = true;
            while (waitOrBreak)
            {
                //List options
                Console.WriteLine("-- Options --");
                Console.WriteLine("List content nodes: l");
                Console.WriteLine("Create new content: c");
                Console.WriteLine("Create Umbraco database schema in empty db: d");
                Console.WriteLine("Quit application: q");

                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("q"))
                    waitOrBreak = false;//Quit the application
                else if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("l"))
                    ListContentNodes(contentService);//Call the method that lists all the content nodes
                else if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("c"))
                    CreateNewContent(contentService, contentTypeService);//Call the method that does the actual creation and saving of the Content object
                else if (string.IsNullOrEmpty(input) == false && input.ToLowerInvariant().Equals("d"))
                    CreateDatabaseSchema(database, databaseContext.DatabaseProvider, application.DataDirectory);
            }
        }

        /// <summary>
        /// Private method to list all content nodes
        /// </summary>
        /// <param name="contentService"></param>
        private static void ListContentNodes(IContentService contentService)
        {
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
        private static void CreateNewContent(IContentService contentService, IContentTypeService contentTypeService)
        {
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
        private static void CreateDatabaseSchema(Database database, DatabaseProviders databaseProvider, string dataDirectory)
        {
            Console.WriteLine("Please note that installing the umbraco database schema requires an empty database configured in config.");
            Console.WriteLine("The 'umbracoConfigurationStatus' under appSettings should be left blank.");
            Console.WriteLine("If you are using Sql Ce an empty Umbraco.sdf file should exist in the DataDictionary.");
            Console.WriteLine("Press y to continue");

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
