set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2ampv4knd3sqpeusuqqi6dnx3agpbe3cpwobv7q5veve -s https://api.nuget.org/v3/index.json
pause
goto begin