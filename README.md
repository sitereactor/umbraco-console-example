Umbraco 6 Console Example
=======================

Sample implementation of a Console application using the Umbraco ContentService for creating and saving content.


Before running this console app please ensure that the "umbracoDbDSN" ConnectionString is pointing to your database.
If you are using Sql Ce please replace "|DataDirectory|" with a real path or alternatively place your database in the debug folder before running the application in debug mode.

You can start off with an empty database and install the umbraco database schema from the application, but this will require that the 'umbracoConfigurationStatus' setting under appSettings is left blank. And if you are using Sql Ce an empty Umbraco.sdf file should exist in the DataDictionary.

This is a very simple implementation so it currently only has 3 options:

- List content nodes
- Create new content
- Create database schema


PS: No WebContext is required
