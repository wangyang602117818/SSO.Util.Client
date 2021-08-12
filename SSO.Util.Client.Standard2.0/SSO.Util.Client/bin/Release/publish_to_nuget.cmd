set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2jnwiefbrm3mj2p6lqfqnjqhvd7cbefpk5esu5c3hk4q -s https://api.nuget.org/v3/index.json
pause
goto begin