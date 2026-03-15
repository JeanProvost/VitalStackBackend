FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Backend.API/Backend.API.csproj", "Backend.API/"]
COPY ["Backend.Core/Backend.Core.csproj", "Backend.Core/"]
COPY ["Backend.Infrastructure/Backend.Infrastructure.csproj", "Backend.Infrastructure/"]
RUN dotnet restore "Backend.API/Backend.API.csproj"

COPY . .
WORKDIR /src/Backend.API
RUN dotnet publish "Backend.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
USER app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Backend.API.dll"]
