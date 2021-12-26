# Development notes

## Architecture

IVsSolutionEvents is implemented so we can hook into the OnAfterCloseSolution event to clear the errorListProvider.

DTE.Events are hooked into for OnBuildProjConfigBegin, OnBuildProjConfigDone, OnBuildBegin, OnBuildDone events.
