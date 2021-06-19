#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Aggregators/eBike.BFF.Aggregator/eBike.BFF.Aggregator.csproj", "src/Aggregators/eBike.BFF.Aggregator/"]
RUN dotnet restore "src/Aggregators/eBike.BFF.Aggregator/eBike.BFF.Aggregator.csproj"
COPY . .
WORKDIR "/src/src/Aggregators/eBike.BFF.Aggregator"
RUN dotnet build "eBike.BFF.Aggregator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "eBike.BFF.Aggregator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eBike.BFF.Aggregator.dll"]