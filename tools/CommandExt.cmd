@echo off
if "~x0"=="%~x0" goto NOCMDEXT 
if "%%~x0"=="%~x0" goto NOCMDEXT
if CmdExtVersion 2 goto CMDEXTV2
goto CMDEXTV1

:CMDEXTV1
echo Command extensions v1 available
goto :EOF

:CMDEXTV2
echo Command extensions v2 or later available
exit /b 0

:NOCMDEXT
echo Command extensions not available
:: END OF FILE