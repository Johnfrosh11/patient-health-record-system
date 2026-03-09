# Install All Required NuGet Packages
# Run this script from the solution root directory

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Installing NuGet Packages for Backend" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set solution root
$solutionRoot = Get-Location

# ============================================================================
# REPOSITORY LAYER
# ============================================================================
Write-Host "1/3 Installing packages for Repository layer..." -ForegroundColor Yellow
cd "$solutionRoot\src\PatientHealthRecord.Repository"

dotnet add package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Microsoft.Extensions.DependencyInjection --version 9.0.0

Write-Host "✓ Repository packages installed" -ForegroundColor Green
Write-Host ""

# ============================================================================
# APPLICATION LAYER
# ============================================================================
Write-Host "2/3 Installing packages for Application layer..." -ForegroundColor Yellow
cd "$solutionRoot\src\PatientHealthRecord.Application"

dotnet add package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.2.1
dotnet add package Microsoft.IdentityModel.Tokens --version 8.2.1
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package Microsoft.AspNetCore.Http.Abstractions --version 2.2.0

Write-Host "✓ Application packages installed" -ForegroundColor Green
Write-Host ""

# ============================================================================
# API LAYER
# ============================================================================
Write-Host "3/3 Installing packages for API layer..." -ForegroundColor Yellow
cd "$solutionRoot\src\PatientHealthRecord.API"

dotnet add package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.0
dotnet add package Serilog.AspNetCore --version 8.0.3
dotnet add package Serilog.Enrichers.CorrelationId --version 3.0.1
dotnet add package Serilog.Sinks.File --version 6.0.0
dotnet add package Swashbuckle.AspNetCore --version 7.2.0
dotnet add package AspNetCore.HealthChecks.SqlServer --version 9.0.0

Write-Host "✓ API packages installed" -ForegroundColor Green
Write-Host ""

# ============================================================================
# RESTORE ALL
# ============================================================================
Write-Host "Restoring all packages..." -ForegroundColor Yellow
cd $solutionRoot
dotnet restore

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ All packages installed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Configure appsettings.json with your database connection string"
Write-Host "2. Run: dotnet ef migrations add InitialMigration --project src/PatientHealthRecord.Repository --startup-project src/PatientHealthRecord.API"
Write-Host "3. Run: dotnet ef database update --project src/PatientHealthRecord.Repository --startup-project src/PatientHealthRecord.API"
Write-Host "4. Run: dotnet run --project src/PatientHealthRecord.API"
Write-Host ""
