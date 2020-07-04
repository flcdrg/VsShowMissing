# Show Missing Files

A Visual Studio 2013/2015/2017/2019 extension to generate warnings or errors for all missing files.

Download the latest version from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=DavidGardiner.ShowMissingFiles)

![GitHub build status](https://github.com/flcdrg/VsShowMissing/workflows/CI/badge.svg)
[![Azure Pipelines build status](https://gardiner.visualstudio.com/Show%20Missing/_apis/build/status/flcdrg.VsShowMissing)](https://gardiner.visualstudio.com/Show%20Missing/_build/latest?definitionId=3)
[![Coverity Scan status](https://scan.coverity.com/projects/5748/badge.svg)](https://scan.coverity.com/projects/5748)

## Development

You will need Visual Studio 2019

### Debugging

In Project Properties, Debug tab, set:

* Select **Start external program** and set value to `C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe` (replace 'Enterprise' with 'Community' or 'Professional' as appropriate)
* Set **command line arguments** to `/RootSuffix Exp`
