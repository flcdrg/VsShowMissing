# Development notes

You will need Visual Studio 2022

## Architecture

IVsSolutionEvents is implemented so we can hook into the OnAfterCloseSolution event to clear the errorListProvider.

DTE.Events are hooked into for OnBuildProjConfigBegin, OnBuildProjConfigDone, OnBuildBegin, OnBuildDone events.

## Debugging (2019)

In Project Properties, Debug tab, set:

* Select **Start external program** and set value to `C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\devenv.exe` (replace 'Enterprise' with 'Community' or 'Professional' as appropriate)
* Set **command line arguments** to `/RootSuffix Exp`

## Debugging (2022)

In Project Properties, Debug tab, set:

* Select **Start external program** and set value to `C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe` (replace 'Enterprise' with 'Community' or 'Professional' as appropriate)
* Set **command line arguments** to `/RootSuffix Exp`
