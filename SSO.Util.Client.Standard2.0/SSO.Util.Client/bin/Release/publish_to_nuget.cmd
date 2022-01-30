set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2jfmh7vxclkuln4izqec6zvw22wnlulh5d2nkudq22gy -s https://api.nuget.org/v3/index.json
pause
goto begin