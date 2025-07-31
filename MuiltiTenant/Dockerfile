# Use the official .NET 7 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["MuiltiTenant.csproj", "./"]
RUN dotnet restore "MuiltiTenant.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "MuiltiTenant.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MuiltiTenant.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 7 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create logs directory
RUN mkdir -p /app/logs

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expose ports
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "MuiltiTenant.dll"]
