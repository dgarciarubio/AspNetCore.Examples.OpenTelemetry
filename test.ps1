Get-ChildItem -Path . -Directory -Filter TestResults -Recurse | 
  Remove-Item -Force -Recurse -ErrorAction SilentlyContinue

dotnet test -- --coverage --coverage-output "coverage.cobertura.xml" --coverage-output-format "cobertura"

dotnet tool update dotnet-reportgenerator-globaltool --global --version 5.4.6
reportgenerator `
  -reports:".\**\TestResults\**\coverage.cobertura.xml" `
  -targetdir:".\TestResults\Report\" `
  -reporttypes:Html