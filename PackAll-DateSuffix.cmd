@SETLOCAL enableextensions enabledelayedexpansion
@ECHO off

RMDIR /S /Q .\artifacts\nuget

REM See http://stackoverflow.com/q/1642677/1143274
FOR /f %%a IN ('WMIC OS GET LocalDateTime ^| FIND "."') DO SET DTS=%%a
SET datetime=%DTS:~0,4%%DTS:~4,2%%DTS:~6,2%-%DTS:~8,2%%DTS:~10,2%%DTS:~12,2%

for /R %%I in (project.json) do if exist %%~fI (
  echo Packing %%~fI
  call _nugetPack.cmd "%%~fI" %datetime% debug
)


ENDLOCAL