FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root

RUN apt-get update \
	&& apt-get install -y libgssapi-krb5-2 \
	&& rm -rf /var/lib/apt/lists/*

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Backend.API/Backend.API.csproj", "Backend.API/"]
COPY ["Backend.Infrastructure/Backend.Infrastructure.csproj", "Backend.Infrastructure/"]
COPY ["Backend.Core/Backend.Core.csproj", "Backend.Core/"]
RUN dotnet restore "./Backend.API/Backend.API.csproj"
COPY . .
WORKDIR "/src/Backend.API"
RUN dotnet build "./Backend.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Backend.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Backend.API.dll"]