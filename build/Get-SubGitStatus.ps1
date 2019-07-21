#Requires -Modules posh-git

[CmdletBinding()]
param (
    [Parameter(Mandatory, ValueFromPipeline)]
    [ValidateNotNullOrEmpty()]
    $Path
)

Set-StrictMode -version Latest
$ErrorActionPreference = 'Stop'

Write-Host "Git Status for $Path"

Push-Location

Get-ChildItem -Path $Path -Directory |
ForEach-Object {

    # Should change Location because Get-GitStatus has BUG with GitDir argument
    $_ |
    Set-Location

    Get-Location |
    Split-Path -Leaf |
    ForEach-Object {
        Write-Host "${_}: " -ForegroundColor Blue -NoNewline
    }

    Get-GitStatus |
    Select-Object -ExpandProperty Working -First 10 |
    ForEach-Object {
        Write-Host "$_ " -ForegroundColor Green -NoNewline
    }

    Write-Host
}

Pop-Location
