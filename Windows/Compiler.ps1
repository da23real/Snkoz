$ErrorActionPreference='Stop'
$compiler831=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
if(!(Test-Path -LiteralPath $compiler831)){$compiler831=Join-Path $env:WINDIR 'Microsoft.NET/Framework/v4.0.30319/csc.exe'}
$sources831=@(Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | ForEach-Object {$_.FullName})
& $compiler831 /nologo /target:winexe /optimize+ /unsafe+ /utf8output /out:"$PSScriptRoot/Snkoz-A01.exe" /reference:Microsoft.CSharp.dll /reference:System.dll /reference:System.Core.dll /reference:System.Security.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Web.Extensions.dll @sources831
if($LASTEXITCODE -ne 0){throw 'Compilation Windows échouée'}
