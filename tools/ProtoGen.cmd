@ECHO OFF

SET thisDir=%~dp0
SET protofile=%1
SET includes=

REM gather up import directories, if any;
REM Note: the import parameters must already be quoted strings if the paths contain spaces
:loop
IF "%~2"=="" GOTO loopend
SET includes=%includes% -I%2
SHIFT
GOTO loop
:loopend

REM set output path to be the same directory as the protofile's location;
REM also check if the output file exists, run script if it is missing
FOR %%a in (%protofile%) DO (
    SET outpath=%%~dpa
    SET csfile=%%~na.cs
)
IF NOT EXIST "%outpath%%csfile%" GOTO :touched

REM check Archive attribute, run script if set
DIR /B /AA %protofile% 1>nul 2>nul && GOTO :touched || GOTO :unchanged

:touched
ECHO "Generating code from %protofile%."

REM append '.' to output directory path because protoc.exe would escape the terminating quote using the path's last backslash
CALL "%thisDir%..\external\protobuf\protoc.exe" -I. %includes% --csharp_out="%outpath%." %protofile%

REM clear Archive attribute
IF NOT ERRORLEVEL 1 ATTRIB -A %protofile%

:unchanged
EXIT /B 0
