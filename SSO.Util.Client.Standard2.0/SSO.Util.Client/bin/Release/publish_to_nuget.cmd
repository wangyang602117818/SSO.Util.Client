set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2cqp4kz2hf7n7lwikapngnpqpo2x7e3lgpocdygcdklm -s https://api.nuget.org/v3/index.json
pause
goto begin