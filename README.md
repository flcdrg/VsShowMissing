# Show Missing Files
A Visual Studio 2013/2015/2017 extension to generate warnings or errors for all missing files.

Download the latest version from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=DavidGardiner.ShowMissingFiles)

[![Build status](https://ci.appveyor.com/api/projects/status/9jvn9qbl4gx58uho?svg=true)](https://ci.appveyor.com/project/DavidGardiner/vsshowmissing)
<a href="https://scan.coverity.com/projects/5748">
  <img alt="Coverity Scan Build Status"
       src="https://scan.coverity.com/projects/5748/badge.svg"/>
</a>

## Development

You will need Visual Studio 2017

### Debugging

In Project Properties, Debug tab, set:

* Select **Start external program** and set value to `C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\devenv.exe` (replace 'Enterprise' with 'Community' or 'Professional' as appropriate)
* Set **command line arguments** to `/RootSuffix Exp`
