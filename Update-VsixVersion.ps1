[cmdletbinding()]
param (
    [Parameter(Position=0, Mandatory=0,ValueFromPipeline=$true)]
    [string[]]$manifestFilePath = ".\source.extension.vsixmanifest",

    [Parameter(Position=1, Mandatory=$true)]
    [Version]$version
)
process {

    $version = New-Object Version ([int]$version.Major),([int]$version.Minor),([System.Math]::Max([int]$version.Build, 0)),$env:APPVEYOR_BUILD_NUMBER

	$env:VSIX_VERSION = "$version"
	
    foreach($manifestFile in $manifestFilePath)
    {
        "Setting VSIX version..." | Write-Host  -ForegroundColor Cyan -NoNewline
        $matches = (Get-ChildItem $manifestFile -Recurse)
        $vsixManifest = $matches[$matches.Count - 1] # Get the last one which matches the top most file in the recursive matches
        [xml]$vsixXml = Get-Content $vsixManifest

        $ns = New-Object System.Xml.XmlNamespaceManager $vsixXml.NameTable
        $ns.AddNamespace("ns", $vsixXml.DocumentElement.NamespaceURI) | Out-Null

        $attrVersion = ""

        if ($vsixXml.SelectSingleNode("//ns:Identity", $ns)){ # VS2012 format
            $attrVersion = $vsixXml.SelectSingleNode("//ns:Identity", $ns).Attributes["Version"]
        }
        elseif ($vsixXml.SelectSingleNode("//ns:Version", $ns)){ # VS2010 format
            $attrVersion = $vsixXml.SelectSingleNode("//ns:Version", $ns)
        }

        $attrVersion.InnerText = $version

        $vsixXml.Save($vsixManifest) | Out-Null

        $version.ToString() | Write-Host -ForegroundColor Green

        # return the values to the pipeline
        New-Object PSObject -Property @{
            'vsixFilePath' = $vsixManifest
            'Version' = $version
        }
    }
}

