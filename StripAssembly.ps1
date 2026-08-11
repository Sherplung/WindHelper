#!/usr/bin/env pwsh

<#
 # Strips a .NET assembly to safely allow the GitHub runner to build the mod.
 #
 # To strip the assembly manually:
 # - install the assembly publicizer with "dotnet tool install -g BepInEx.AssemblyPublicizer.Cli"
 # - strip the assembly with "assembly-publicizer --strip-only AssemblyName.dll"
 # - put AssemblyName-publicized.dll in lib-stripped
 # - remove the "-publicized" suffix from the assembly name
 #>

param (
    [Parameter(Mandatory = $true)]
    [string] $AssemblyName
)

# Set working directory to lib-stripped, relative to where the script is
$TargetDirectory = [System.IO.Path]::Combine("Source", "lib-stripped")
Push-Location "$PSScriptRoot/$TargetDirectory"

try
{
    # Ensure that the assembly exists in lib-stripped
    if (-not (Test-Path $AssemblyName))
    {
        Write-Error "`"$([System.IO.Path]::Combine($TargetDirectory, $AssemblyName))`" does not exist."
        exit 1
    }

    # Ensure that the assembly publicizer is present
    $PublicizerToolName = "BepInEx.AssemblyPublicizer.Cli"
    if (-not (dotnet tool list -g | Select-String $PublicizerToolName))
    {
        Write-Host "Assembly publicizer not present, installing $PublicizerToolName..."
        dotnet tool install -g $PublicizerToolName
        if (-not $?)
        {
            Write-Error "Failed to install $PublicizerToolName."
            exit 1
        }
    }

    # Strip the assembly
    assembly-publicizer --strip-only $AssemblyName
    if (-not $?)
    {
        Write-Error "Failed to publicize `"$AssemblyName`"."
        exit 1
    }

    # Replace the stripped file with the publicized version
    $StrippedAssemblyName =
        [System.IO.Path]::GetFileNameWithoutExtension($AssemblyName) `
        + "-publicized" `
        + [System.IO.Path]::GetExtension($AssemblyName)

    Remove-Item $AssemblyName
    Rename-Item $StrippedAssemblyName $AssemblyName

    Write-Host "Successfully stripped `"$AssemblyName`"."
}
finally
{
    # Return to the previous location when exiting
    Pop-Location
}
