#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Workers/eBike.Workers.Processing/eBike.Workers.Processing.csproj", "src/Workers/eBike.Workers.Processing/"]
COPY ["src/Commons/eBike.Commons/eBike.Commons.csproj", "src/Commons/eBike.Commons/"]
RUN dotnet restore "src/Workers/eBike.Workers.Processing/eBike.Workers.Processing.csproj"
COPY . .
WORKDIR "/src/src/Workers/eBike.Workers.Processing"
RUN dotnet build "eBike.Workers.Processing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "eBike.Workers.Processing.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eBike.Workers.Processing.dll"]