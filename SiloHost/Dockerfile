FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.6-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.302-buster AS build

WORKDIR /Interfaces
COPY Interfaces/Interfaces.csproj ./
RUN dotnet restore "Interfaces.csproj"
COPY Interfaces/ ./

WORKDIR /Grains
COPY Grains/Grains.csproj ./
RUN dotnet restore "Grains.csproj"
COPY Grains/ ./

WORKDIR /SiloHost
COPY SiloHost/SiloHost.csproj ./
RUN dotnet restore "SiloHost.csproj" 
COPY SiloHost/ ./
RUN dotnet build "SiloHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SiloHost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SiloHost.dll"]