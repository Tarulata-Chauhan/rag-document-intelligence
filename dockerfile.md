# Use the official lightweight ASP.NET 8 runtime as a base

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the full .NET 8 SDK for the build phase

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies

# Doing this before copying the rest of the code leverages Docker caching

COPY \["RAG_API.csproj", "./"\]
RUN dotnet restore "./RAG_API.csproj"

# Copy the remaining source code and build the application

COPY . .
WORKDIR "/src/"
RUN dotnet build "./RAG_API.csproj" -c \$BUILD_CONFIGURATION -o /app/build

# Publish the application to a folder

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RAG_API.csproj" -c \$BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage/image: Copy the published output into the lightweight base image

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the entry point for the container

ENTRYPOINT \["dotnet", "RAG_API.dll"\]