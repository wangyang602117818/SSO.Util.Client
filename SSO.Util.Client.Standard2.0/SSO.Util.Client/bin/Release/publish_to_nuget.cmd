set input=
set /p input=please input version number:
echo publish: %input%
dotnet nuget push SSO.Util.Client.%input%.nupkg -k oy2oblciqmu45epz2seqr7f7s5azwire7mnqsmialc2ari -s https://api.nuget.org/v3/index.json
pause
goto begin