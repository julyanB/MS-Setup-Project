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
COPY ["EmployeeManagementService.Startup/EmployeeManagementService.Startup.csproj", "EmployeeManagementService.Startup/"]
COPY ["EmployeeManagementService.Web/EmployeeManagementService.Web.csproj", "EmployeeManagementService.Web/"]
COPY ["EmployeeManagementService.Application/EmployeeManagementService.Application.csproj", "EmployeeManagementService.Application/"]
COPY ["EmployeeManagementService.Domain/EmployeeManagementService.Domain.csproj", "EmployeeManagementService.Domain/"]
COPY ["EmployeeManagementService.Infrastructure/EmployeeManagementService.Infrastructure.csproj", "EmployeeManagementService.Infrastructure/"]
RUN dotnet restore "EmployeeManagementService.Startup/EmployeeManagementService.Startup.csproj"
COPY . .
WORKDIR "/src/EmployeeManagementService.Startup"
RUN dotnet build "EmployeeManagementService.Startup.csproj" -c Release -o /app/build

FROM build AS publish
ARG VERSION=1.0.0
RUN dotnet publish "EmployeeManagementService.Startup.csproj" /p:Version=$VERSION -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeManagementService.Startup.dll"]
