#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["BStockBot.Api/BStockBot.Api.csproj", "BStockBot.Api/"]
RUN dotnet restore "BStockBot.Api/BStockBot.Api.csproj"
COPY . .
WORKDIR "/src/BStockBot.Api"
RUN dotnet build "BStockBot.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BStockBot.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get update && apt-get install -y apt-transport-https
RUN apt-get -y install fontconfig
COPY fonts/ ~/.fonts
COPY fonts/ /usr/share/fonts
COPY fonts/ /usr/share/fonts/truetype
RUN fc-cache -f -v
ENTRYPOINT ["dotnet", "BStockBot.Api.dll"]