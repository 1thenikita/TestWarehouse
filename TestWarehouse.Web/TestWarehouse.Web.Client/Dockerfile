FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TestWarehouse.Web/TestWarehouse.Web.Client/TestWarehouse.Web.Client.csproj", "TestWarehouse.Web/TestWarehouse.Web.Client/"]
RUN dotnet restore "TestWarehouse.Web/TestWarehouse.Web.Client/TestWarehouse.Web.Client.csproj"
COPY . .
WORKDIR "/src/TestWarehouse.Web/TestWarehouse.Web.Client"
RUN dotnet build "TestWarehouse.Web.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TestWarehouse.Web.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestWarehouse.Web.Client.dll"]
