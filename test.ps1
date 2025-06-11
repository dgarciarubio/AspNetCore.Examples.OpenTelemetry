param(
	[switch] $RunBenchmarks = $false
	[string] $Configuration = "Release"
) 

Get-ChildItem -Path . -Directory -Filter TestResults -Recurse | 
  Remove-Item -Force -Recurse -ErrorAction SilentlyContinue

dotnet test ./TelemetryServices.Tests/                --configuration $Configuration -- --coverage --coverage-output "coverage.cobertura.xml" --coverage-output-format "cobertura"
dotnet test ./TelemetryServices.OpenTelemetry.Tests/  --configuration $Configuration -- --coverage --coverage-output "coverage.cobertura.xml" --coverage-output-format "cobertura"

if ($RunBenchmarks)
{
	dotnet test ./TelemetryServices.Benchmarks.Tests/ --configuration Release
}

dotnet tool update dotnet-reportgenerator-globaltool --global --version 5.4.6
reportgenerator `
  -reports:".\**\TestResults\**\coverage.cobertura.xml" `
  -targetdir:".\TestResults\Report\" `
  -reporttypes:Html