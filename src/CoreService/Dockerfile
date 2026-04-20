#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses it for debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apk update && \
    apk upgrade --no-cache && \
    apk add --no-cache tzdata icu-libs icu-data-full

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CoreService.Startup/CoreService.Startup.csproj", "CoreService.Startup/"]
COPY ["CoreService.Web/CoreService.Web.csproj", "CoreService.Web/"]
COPY ["CoreService.Application/CoreService.Application.csproj", "CoreService.Application/"]
COPY ["CoreService.Domain/CoreService.Domain.csproj", "CoreService.Domain/"]
COPY ["CoreService.Infrastructure/CoreService.Infrastructure.csproj", "CoreService.Infrastructure/"]
RUN dotnet restore "CoreService.Startup/CoreService.Startup.csproj"
COPY . .
WORKDIR "/src/CoreService.Startup"
RUN dotnet build "CoreService.Startup.csproj" -c Release -o /app/build

FROM build AS publish
ARG VERSION=1.0.0
RUN dotnet publish "CoreService.Startup.csproj" /p:Version=$VERSION -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreService.Startup.dll"]
