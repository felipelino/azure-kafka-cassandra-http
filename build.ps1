dotnet build
dotnet publish -c Release
if(Test-path "$pwd/publish.zip") { Remove-item "$pwd/publish.zip" }
Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory("$pwd/POC.FunctionApp/bin/Release/netcoreapp3.1/publish/", "$pwd\publish.zip")