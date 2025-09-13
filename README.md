# Show Missing Files

A Visual Studio extension that looks for file references in projects that refer to files that do not physically exist on the file system. It can also optionally warn about files on disk that are not included in a project.

You can trigger the extension by performing a build on the solution or an individual project.

Missing files are listed in the Error List window. Double-click on an item in the list to navigate to the relevant location in the Solution Explorer.

Right-click on one or more similar items to get a context menu with options to fix the selected warnings.

![Example](https://raw.githubusercontent.com/flcdrg/VsShowMissing/main/example.png)

Visual Studio version-specific releases are published to the Visual Studio Marketplace:

- [Show Missing Files](https://marketplace.visualstudio.com/items?itemName=DavidGardiner.ShowMissingFiles) (Visual Studio 2013, 2015, 2017)
- [Show Missing Files 2019](https://marketplace.visualstudio.com/items?itemName=DavidGardiner.ShowMissing2019) (Visual Studio 2019)
- [Show Missing Files 2022](https://marketplace.visualstudio.com/items?itemName=DavidGardiner.ShowMissing2022) (Visual Studio 2022)

![GitHub build status](https://github.com/flcdrg/VsShowMissing/workflows/CI/badge.svg)
[![Azure Pipelines build status](https://gardiner.visualstudio.com/Show%20Missing/_apis/build/status/flcdrg.VsShowMissing)](https://gardiner.visualstudio.com/Show%20Missing/_build/latest?definitionId=3)
[![Coverity Scan status](https://img.shields.io/coverity/scan/5748)](https://scan.coverity.com/projects/5748)
