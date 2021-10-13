set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2fwss7p32dbkwfo4nqzpb5eg2xzczuphe5grikqlolwa -s https://api.nuget.org/v3/index.json
pause
goto begin