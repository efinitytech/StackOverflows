Set-Location StackOverflow
dotnet pack -c Release
if (( @(dotnet tool list -g) -match "stackoverflow*" ).Length -gt 0) {
    dotnet tool update stackoverflow -g
}
else {
    dotnet tool install --global --add-source ./nupkg stackoverflow
}
so --help