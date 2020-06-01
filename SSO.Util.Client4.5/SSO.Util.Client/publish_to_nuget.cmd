set input=
set /p input=please input version number:
echo publish: %input%
nuget pack SSO.Util.Client.csproj
nuget push SSO.Util.Client.%input%.nupkg oy2kbrgpmg5eqjn3j66aqmlzlm75jzce3jxp5urkwtcrqm -Source https://www.nuget.org/api/v2/package