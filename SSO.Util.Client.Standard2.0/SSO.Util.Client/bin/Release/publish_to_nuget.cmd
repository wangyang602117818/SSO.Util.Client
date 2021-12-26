set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2edparw5fixp24yj4qfxkmo5zhwtnebmh6owwlxernym -s https://api.nuget.org/v3/index.json
pause
goto begin