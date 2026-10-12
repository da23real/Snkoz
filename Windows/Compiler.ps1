param([string]$OutputDirectory='')
$ErrorActionPreference='Stop'
if(!$OutputDirectory){$OutputDirectory=Join-Path (Split-Path $PSScriptRoot) 'Build/Windows'}
New-Item -ItemType Directory -Force $OutputDirectory | Out-Null
$compilerR21=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
if(!(Test-Path -LiteralPath $compilerR21)){$compilerR21=Join-Path $env:WINDIR 'Microsoft.NET/Framework/v4.0.30319/csc.exe'}
$vbR21=Join-Path (Split-Path $compilerR21) 'Microsoft.VisualBasic.dll'
$sourcesR21=@(Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | ForEach-Object FullName)
$outputR21=Join-Path $OutputDirectory 'Snkoz-VA01.15-R21.exe'
& $compilerR21 /nologo /target:winexe /main:UrbanKaoz.DisplayRecorder.Program /unsafe+ /utf8output "/win32icon:$PSScriptRoot/Assets/Snkoz.ico" "/out:$outputR21" /reference:Microsoft.CSharp.dll /reference:System.dll /reference:System.Core.dll /reference:System.Security.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Web.Extensions.dll "/reference:$vbR21" @sourcesR21
if($LASTEXITCODE -ne 0){throw 'Compilation Windows R21 échouée'}
Write-Output $outputR21
