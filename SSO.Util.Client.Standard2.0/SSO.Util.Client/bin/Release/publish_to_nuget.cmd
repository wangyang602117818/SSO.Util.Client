set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2lorj5nfyprvkeasjq3mqgnwcw47gulcx52hncez7soa -s https://api.nuget.org/v3/index.json
pause
goto begin