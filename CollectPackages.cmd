
@SETLOCAL enableextensions enabledelayedexpansion
@ECHO off

RMDIR /S /Q artifacts\nuget
MKDIR artifacts\nuget

FOR /R %%I IN (*.csproj) DO IF EXIST %%~fI (
  XCOPY "%%~dpIbin\debug\*.nupkg" "artifacts\nuget"
)

ENDLOCAL

PAUSE