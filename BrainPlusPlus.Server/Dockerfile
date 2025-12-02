# Stage 1: Build React frontend
FROM node:20-alpine AS frontend-build
WORKDIR /src/frontend

COPY brainplusplus.client/package*.json ./
RUN npm install

COPY brainplusplus.client/ ./
RUN npm run build

# Stage 2: Build .NET backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

COPY BrainPlusPlus.Server/*.csproj ./BrainPlusPlus.Server/
RUN dotnet restore ./BrainPlusPlus.Server/BrainPlusPlus.Server.csproj

COPY BrainPlusPlus.Server/ ./BrainPlusPlus.Server/

COPY --from=frontend-build /src/frontend/dist ./BrainPlusPlus.Server/wwwroot

WORKDIR /src/BrainPlusPlus.Server
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y \
    g++ \
    && rm -rf /var/lib/apt/lists/*

COPY --from=backend-build /app/publish .

EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "BrainPlusPlus.Server.dll"]