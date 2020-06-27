set input=
set /p input=please input version number:
echo publish: %input%
nuget pack SSO.Util.Client.csproj
nuget push SSO.Util.Client.%input%.nupkg oy2lqlbzp6jcu44wtwba5scf6jca7dzu5jjcuquhqbbyou -Source https://www.nuget.org/api/v2/package
pause
goto begin