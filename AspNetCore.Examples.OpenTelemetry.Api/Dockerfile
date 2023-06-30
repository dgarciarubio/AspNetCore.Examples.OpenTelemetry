#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AspNetCore.Examples.OpenTelemetry.Api/AspNetCore.Examples.OpenTelemetry.Api.csproj", "AspNetCore.Examples.OpenTelemetry.Api/"]
RUN dotnet restore "AspNetCore.Examples.OpenTelemetry.Api/AspNetCore.Examples.OpenTelemetry.Api.csproj"
COPY . .
WORKDIR "/src/AspNetCore.Examples.OpenTelemetry.Api"
RUN dotnet build "AspNetCore.Examples.OpenTelemetry.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AspNetCore.Examples.OpenTelemetry.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspNetCore.Examples.OpenTelemetry.Api.dll"]