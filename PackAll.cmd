@SETLOCAL enableextensions enabledelayedexpansion
@ECHO off

RMDIR /S /Q .\artifacts\nuget

for /R %%I in (project.json) do if exist %%~fI (
  echo Packing %%~fI
  call _nugetPack.cmd "%%~fI" "" debug
)


ENDLOCAL