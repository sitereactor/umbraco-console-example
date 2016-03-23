## Addition to core project

Runs from site root folder. Creates appdomain with Umbraco.  
Commandline argument for fully qualified name of class implementing following contract:  
`public void Execute(Umbraco.Core.ApplicationContext context)`  
or run option 'e' and enter the classname.

## Core project readme

Umbraco 6 + 7 Console Example
=======================

Sample implementation of a Console application using the Umbraco ContentService for creating and saving content.


*Applies to the version 6 example*: Before running this console app please ensure that the "umbracoDbDSN" ConnectionString is pointing to your database.
If you are using Sql Ce please replace "|DataDirectory|" with a real path or alternatively place your database in the debug folder before running the application in debug mode.

You can start off with an empty database and install the umbraco database schema from the application, but this will require that the 'umbracoConfigurationStatus' setting under appSettings is left blank. And if you are using Sql Ce an empty Umbraco.sdf file should exist in the DataDictionary.

*Applies to the version 7 example*: This example assumes that the console executable is placed in the bin folder along side all the Umbraco assemblies. So the structure corresponds to that of a regular Umbraco site in that it assumes an AppData and Config folder to be present in the base directory. When using a Sql Ce database the "|DataDirectory|" part will be updated to the App_Data folder in the base directory and a new database will be created here if one doesn't already exists.

This is a very simple implementation so it currently only has 3 options:

- List content nodes
- Create new content
- Create database schema


**NOTE:** The Umbraco 6 version of this console example is tagged with 'Umbraco-v.6.0.1' in the master branch. The latest commits found in the master branch is using version 7.0.2 of Umbraco.

The main difference between the version 6 and 7 examples is an updated use of a base directory, which tells Umbraco to use a specific path as its base. This path is typically used to save media and other files to a specific set of folders.

A special note about the version 7 example: Because all config files are now standard .NET config files they have to be placed side-by-side or in a subfolder of the web.config file (app.config for this console app) in order for the console to run. All config files placed in the \root\Config folder is therefor copied into the bin folder on application start.

*PS: No WebContext is required*