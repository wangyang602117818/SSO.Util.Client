set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2pdsu4qb54gh7wjz6qhgdgdgh2xlbeloado4l3rqpefq -s https://api.nuget.org/v3/index.json
pause
goto begin