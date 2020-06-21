set input=
set /p input=please input version number:
echo publish: %input%
nuget pack SSO.Util.Client.csproj
nuget push SSO.Util.Client.%input%.nupkg oy2fxx2immv46rslk2nal5cnt6xrd5buhgv6iq2pf2ftba -Source https://www.nuget.org/api/v2/package