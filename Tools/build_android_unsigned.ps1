param([string]$Sdk="$env:LOCALAPPDATA\Android\Sdk",[string]$Jdk=$env:JAVA_HOME)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$buildDir=Join-Path $taskRoot 'BuildAndroid'
$distDir=Join-Path $taskRoot 'Android'
$sourceRoot=Join-Path $distDir 'app/src/main'
$toolsDir=Join-Path $Sdk 'build-tools/35.0.0'
New-Item -ItemType Directory -Force -Path "$buildDir/classes","$buildDir/dex" | Out-Null
Copy-Item -LiteralPath "$Sdk/platforms/android-35/android.jar" -Destination "$buildDir/android.jar" -Force
$androidJar="$buildDir/android.jar"
function Run-Checked([string]$Program,[string[]]$Arguments){& $Program @Arguments;if($LASTEXITCODE -ne 0){throw "Échec $Program : $LASTEXITCODE"}}
$sources=@(Get-ChildItem -LiteralPath "$sourceRoot/java" -Filter '*.java' -Recurse | ForEach-Object FullName)
Run-Checked "$Jdk/bin/javac.exe" (@('-encoding','UTF-8','-source','8','-target','8','-classpath',$androidJar,'-d',"$buildDir/classes")+$sources)
[xml]$manifest=Get-Content -LiteralPath "$sourceRoot/AndroidManifest.xml" -Raw
$manifest.DocumentElement.SetAttribute('package','fr.roy.obsusb')
$manifest.Save("$buildDir/AndroidManifest.xml")
Run-Checked "$toolsDir/aapt2.exe" @('compile','--dir',"$sourceRoot/res",'-o',"$buildDir/resources.zip")
Run-Checked "$toolsDir/aapt2.exe" @('link','-I',$androidJar,'--manifest',"$buildDir/AndroidManifest.xml",'--min-sdk-version','26','--target-sdk-version','35','--version-code','100001','--version-name','V1','-o',"$buildDir/unsigned.apk","$buildDir/resources.zip")
$classes=@(Get-ChildItem -LiteralPath "$buildDir/classes" -Filter '*.class' -Recurse | ForEach-Object FullName)
Run-Checked "$Jdk/bin/java.exe" (@('-cp',"$toolsDir/lib/d8.jar",'com.android.tools.r8.D8','--lib',$androidJar,'--min-api','26','--output',"$buildDir/dex")+$classes)
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive=[IO.Compression.ZipFile]::Open("$buildDir/unsigned.apk",[IO.Compression.ZipArchiveMode]::Update)
try{[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,"$buildDir/dex/classes.dex",'classes.dex')|Out-Null}finally{$archive.Dispose()}
Run-Checked "$toolsDir/zipalign.exe" @('-f','-p','4',"$buildDir/unsigned.apk","$buildDir/aligned.apk")
