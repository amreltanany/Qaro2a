# Clears application data using ConnectionStrings__DefaultConnection from environment.
# Usage:
#   $env:ConnectionStrings__DefaultConnection = "Server=tcp:....database.windows.net,1433;..."
#   .\Scripts\ClearDatabase.ps1

$ErrorActionPreference = "Stop"
$conn = $env:ConnectionStrings__DefaultConnection
if ([string]::IsNullOrWhiteSpace($conn)) {
    Write-Error "Set ConnectionStrings__DefaultConnection first."
}

$sqlPath = Join-Path $PSScriptRoot "ClearApplicationData.sql"
$sql = Get-Content $sqlPath -Raw

Add-Type -AssemblyName "Microsoft.Data.SqlClient"
$connection = New-Object Microsoft.Data.SqlClient.SqlConnection($conn)
$connection.Open()
try {
    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    $command.ExecuteNonQuery() | Out-Null
    Write-Host "Database cleared successfully."
}
finally {
    $connection.Close()
}
