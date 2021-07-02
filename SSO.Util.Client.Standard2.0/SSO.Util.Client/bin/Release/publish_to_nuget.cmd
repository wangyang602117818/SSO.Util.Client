set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2cptu4m626psjqfj2ahiaq7jtkoezezeizye5qozrvvi -s https://api.nuget.org/v3/index.json
pause
goto begin