set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2crpmgebql2hihurcapzlton5gto4uznrxjoroba7lfi -s https://api.nuget.org/v3/index.json
pause
goto begin