
@SETLOCAL enableextensions enabledelayedexpansion
@ECHO off

RMDIR /S /Q nuget
MKDIR nuget

PUSHD ..

FOR /R %%I IN (*.csproj) DO IF EXIST %%~fI (
  XCOPY "%%~dpIbin\debug\*.nupkg" "artifacts\nuget"
)

POPD

ENDLOCAL

PAUSE