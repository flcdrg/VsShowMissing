# Show Missing Files
A Visual Studio 2013/2015 extension to generate warnings or errors for all missing files.

Download the latest version from the [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/900b48cc-52b5-4afa-b4db-f1c3655c32aa)

[![Build status](https://ci.appveyor.com/api/projects/status/9jvn9qbl4gx58uho?svg=true)](https://ci.appveyor.com/project/DavidGardiner/vsshowmissing)
<a href="https://scan.coverity.com/projects/5748">
  <img alt="Coverity Scan Build Status"
       src="https://scan.coverity.com/projects/5748/badge.svg"/>
</a>

## Development

### Debugging

In Project Properties, Debug tab, set:

* Select **Start external program** and set value to `C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe`
* Set **command line arguments** to `/RootSuffix Exp`