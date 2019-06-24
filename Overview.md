A Visual Studio 2019 extension that looks for file references in projects that refer to files that do not physically exist on the file system. It can also optionally warn about files on disk that are not included in a project. There's a [separate version for 2013-2017](https://marketplace.visualstudio.com/items?itemName=DavidGardiner.ShowMissingFiles).

You can trigger the extension by performing a build on the solution or an individual project.

Missing files are listed in the Error List window. Double-click on an item in the list to navigate to the relevant location in the Solution Explorer.

Right-click on one or more similar items to get a context menu with options to fix the selected warnings.

This is the first release for 2019 (porting the code over from the 2013-2017 version). If you encounter problems, please raise an [issue on GitHub](https://github.com/flcdrg/VsShowMissing/issues).
