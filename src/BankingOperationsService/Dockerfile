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
COPY ["BankingOperationsService.Startup/BankingOperationsService.Startup.csproj", "BankingOperationsService.Startup/"]
COPY ["BankingOperationsService.Web/BankingOperationsService.Web.csproj", "BankingOperationsService.Web/"]
COPY ["BankingOperationsService.Application/BankingOperationsService.Application.csproj", "BankingOperationsService.Application/"]
COPY ["BankingOperationsService.Domain/BankingOperationsService.Domain.csproj", "BankingOperationsService.Domain/"]
COPY ["BankingOperationsService.Infrastructure/BankingOperationsService.Infrastructure.csproj", "BankingOperationsService.Infrastructure/"]
RUN dotnet restore "BankingOperationsService.Startup/BankingOperationsService.Startup.csproj"
COPY . .
WORKDIR "/src/BankingOperationsService.Startup"
RUN dotnet build "BankingOperationsService.Startup.csproj" -c Release -o /app/build

FROM build AS publish
ARG VERSION=1.0.0
RUN dotnet publish "BankingOperationsService.Startup.csproj" /p:Version=$VERSION -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankingOperationsService.Startup.dll"]
