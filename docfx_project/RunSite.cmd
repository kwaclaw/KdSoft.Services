echo off
start /B http://localhost:8086 && start docfx.exe serve _site -p 8086
exit

