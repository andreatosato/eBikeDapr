#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Services/eBike.Services.Bikes/eBike.Services.Bikes.csproj", "src/Services/eBike.Services.Bikes/"]
COPY ["src/Commons/eBike.Commons/eBike.Commons.csproj", "src/Commons/eBike.Commons/"]
RUN dotnet restore "src/Services/eBike.Services.Bikes/eBike.Services.Bikes.csproj"
COPY . .
WORKDIR "/src/src/Services/eBike.Services.Bikes"
RUN dotnet build "eBike.Services.Bikes.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "eBike.Services.Bikes.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eBike.Services.Bikes.dll"]