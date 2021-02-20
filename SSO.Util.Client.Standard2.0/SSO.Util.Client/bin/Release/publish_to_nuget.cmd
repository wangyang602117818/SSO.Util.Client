set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2ipmz6fxqvcoe4g6gajck3spxoul3ube2wb5ymgytziy -s https://api.nuget.org/v3/index.json
pause
goto begin