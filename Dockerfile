FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["BinFlow.API/BinFlow.API.csproj", "BinFlow.API/"]
COPY ["BinFlow.Shared/BinFlow.Shared.csproj", "BinFlow.Shared/"]

# Restore dependencies
RUN dotnet restore "BinFlow.API/BinFlow.API.csproj"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/BinFlow.API"
RUN dotnet build "BinFlow.API.csproj" -c Release -o /app/build
RUN dotnet publish "BinFlow.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BinFlow.API.dll"]