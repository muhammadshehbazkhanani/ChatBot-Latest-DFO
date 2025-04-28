# Base stage for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage with Node.js and .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install Node.js for Angular build
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g @angular/cli

# Copy project files
COPY AngularApp1.Server/AngularApp1.Server.csproj AngularApp1.Server/
COPY angularapp1.client/package*.json angularapp1.client/

# Restore dependencies
RUN dotnet restore AngularApp1.Server/AngularApp1.Server.csproj
RUN cd angularapp1.client && npm install

# Copy remaining files
COPY . .

# Build Angular client
WORKDIR /src/angularapp1.client
RUN npm run build -- --output-path=/app/wwwroot

# Build .NET backend
WORKDIR /src/AngularApp1.Server
RUN dotnet build AngularApp1.Server.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish AngularApp1.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /app/wwwroot ./wwwroot
ENTRYPOINT ["dotnet", "AngularApp1.Server.dll"]
