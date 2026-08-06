@echo off
echo Publishing PierreLauncher as Self-Contained Executable...
cd PierreLauncher
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "..\Publish"
cd ..
echo Done.
echo Creating Setup...
"C:\Users\mymai\AppData\Local\Programs\Inno Setup 6\ISCC.exe" "Installer.iss"
echo Setup created in Output/ folder!
