#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AspNetCore.OpenTelemetry.Example.Api/AspNetCore.OpenTelemetry.Example.Api.csproj", "AspNetCore.OpenTelemetry.Example.Api/"]
RUN dotnet restore "AspNetCore.OpenTelemetry.Example.Api/AspNetCore.OpenTelemetry.Example.Api.csproj"
COPY . .
WORKDIR "/src/AspNetCore.OpenTelemetry.Example.Api"
RUN dotnet build "AspNetCore.OpenTelemetry.Example.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AspNetCore.OpenTelemetry.Example.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AspNetCore.OpenTelemetry.Example.Api.dll"]