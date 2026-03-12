FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8003
# Install curl for health check
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8003/health || exit 1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RehberlikSistemi.Web/RehberlikSistemi.Web.csproj", "./"]
RUN dotnet restore "RehberlikSistemi.Web/RehberlikSistemi.Web.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "RehberlikSistemi.Web/RehberlikSistemi.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RehberlikSistemi.Web/RehberlikSistemi.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8003
ENV TZ=Europe/Istanbul
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
ENTRYPOINT ["dotnet", "RehberlikSistemi.Web.dll"]
