Set-Location StackOverflow
dotnet pack -c Release
dotnet tool install --global --add-source ./nupkg StackOverflow