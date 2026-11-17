FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["API_HealthCheck/API_HealthCheck.csproj", "API_HealthCheck/"]
RUN dotnet restore "API_HealthCheck/API_HealthCheck.csproj"
COPY . .
RUN dotnet publish "API_HealthCheck/API_HealthCheck.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "API_HealthCheck.dll"]
