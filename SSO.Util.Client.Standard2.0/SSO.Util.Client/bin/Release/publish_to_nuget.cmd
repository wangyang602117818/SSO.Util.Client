set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2oifgw7am7tlxzdwuqdw4sz7oss2yu7ewgg7hnzvprna -s https://api.nuget.org/v3/index.json
pause
goto begin