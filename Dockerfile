# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/PaymentsAPI.Api/PaymentsAPI.Api.csproj", "PaymentsAPI.Api/"]
COPY ["src/PaymentsAPI.Application/PaymentsAPI.Application.csproj", "PaymentsAPI.Application/"]
COPY ["src/PaymentsAPI.Domain/PaymentsAPI.Domain.csproj", "PaymentsAPI.Domain/"]
COPY ["src/PaymentsAPI.Infrastructure/PaymentsAPI.Infrastructure.csproj", "PaymentsAPI.Infrastructure/"]

RUN dotnet restore "PaymentsAPI.Api/PaymentsAPI.Api.csproj"

# Copy all source code
COPY src/ .

# Build and publish
WORKDIR "/src/PaymentsAPI.Api"
RUN dotnet build "PaymentsAPI.Api.csproj" -c Release -o /app/build
RUN dotnet publish "PaymentsAPI.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "PaymentsAPI.Api.dll"]
