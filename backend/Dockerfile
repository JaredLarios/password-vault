# BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Set working directory
WORKDIR /src

# Copy package files
COPY *.csproj ./

# Install dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Expose port (adjust if needed)
EXPOSE 8020

RUN dotnet publish -c Release -o /app/publish

# RUNTIME STAGE
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8020

# Expose API port
EXPOSE 8020

# Run app
ENTRYPOINT ["dotnet", "PasswordVaultAPI.dll"]