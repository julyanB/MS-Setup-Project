#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses it for debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apk update && \
    apk upgrade --no-cache && \
    apk add --no-cache tzdata

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Gateway.csproj", "./"]
RUN dotnet restore "Gateway.csproj"
COPY . .
RUN dotnet build "Gateway.csproj" -c Release -o /app/build

FROM build AS publish
ARG VERSION=1.0.0
RUN dotnet publish "Gateway.csproj" /p:Version=$VERSION -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.dll"]
