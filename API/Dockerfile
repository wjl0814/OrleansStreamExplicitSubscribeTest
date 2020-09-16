FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.6-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.302-buster AS build

WORKDIR /Interfaces
COPY Interfaces/Interfaces.csproj ./
RUN dotnet restore "Interfaces.csproj"
COPY Interfaces/ ./

WORKDIR /API
COPY API/API.csproj ./
RUN dotnet restore "API.csproj"
COPY API/ ./
RUN dotnet build "API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]