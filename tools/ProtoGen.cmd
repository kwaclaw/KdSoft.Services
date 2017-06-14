@echo off

SET thisDir=%~dp0
SET includes=%1
SET protofile=%2

echo %protofile%

REM check Archive attribute, run script if set
DIR /B /AA %protofile% 1>nul 2>nul && goto :touched || goto :unchanged

:touched
echo "Generating code from %protofile%."

call "%thisDir%\..\external\protobuf\protoc.exe" -I=%includes% --csharp_out=. %protofile%

REM clear Archive attribute
if not errorlevel 1 attrib -A %protofile%

:unchanged
exit /B 0
