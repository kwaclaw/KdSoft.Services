for /d /r . %%d in (artifacts\*, bin, obj) do @if exist "%%d" echo "%%d" && "tools\SuperDelete.exe" "%%d"

pause


