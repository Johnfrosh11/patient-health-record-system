# ── Stage 1: Build ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY src/PatientHealthRecord.API/PatientHealthRecord.API.csproj                       src/PatientHealthRecord.API/
COPY src/PatientHealthRecord.Application/PatientHealthRecord.Application.csproj       src/PatientHealthRecord.Application/
COPY src/PatientHealthRecord.Domain/PatientHealthRecord.Domain.csproj                 src/PatientHealthRecord.Domain/
COPY src/PatientHealthRecord.Infrastructure/PatientHealthRecord.Infrastructure.csproj src/PatientHealthRecord.Infrastructure/
COPY src/PatientHealthRecord.Repository/PatientHealthRecord.Repository.csproj         src/PatientHealthRecord.Repository/
COPY src/PatientHealthRecord.Utilities/PatientHealthRecord.Utilities.csproj           src/PatientHealthRecord.Utilities/

RUN dotnet restore src/PatientHealthRecord.API/PatientHealthRecord.API.csproj

# Copy everything else and publish
COPY . .
RUN dotnet publish src/PatientHealthRecord.API/PatientHealthRecord.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ── Stage 2: Runtime ─────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# Railway injects PORT at runtime; ASPNETCORE_URLS is overridden in Program.cs
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PatientHealthRecord.API.dll"]
