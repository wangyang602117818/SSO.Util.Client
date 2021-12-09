set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2fwlapk2mi4rcw47pqomavjei4bube3eicns74rmrl4m -s https://api.nuget.org/v3/index.json
pause
goto begin