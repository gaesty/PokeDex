# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file(s) and restore as distinct layers for better caching
COPY ["BourgPalette/BourgPalette.csproj", "BourgPalette/"]
RUN dotnet restore "BourgPalette/BourgPalette.csproj"

# Copy the rest of the source and publish
COPY . .
WORKDIR /src/BourgPalette
RUN dotnet publish "BourgPalette.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BourgPalette.dll"]
