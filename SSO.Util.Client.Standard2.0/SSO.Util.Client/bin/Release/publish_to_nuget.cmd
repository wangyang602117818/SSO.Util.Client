set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2oaave6ohypvhixayqww5ortzhmemevakijz4ns32g2m -s https://api.nuget.org/v3/index.json
pause
goto begin