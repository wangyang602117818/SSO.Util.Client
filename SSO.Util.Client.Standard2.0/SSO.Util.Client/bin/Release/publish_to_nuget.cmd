set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2b5xmnam2hq3cnjcxqlbqwk5464ryeffl6fzjg4gvt5a -s https://api.nuget.org/v3/index.json
pause
goto begin