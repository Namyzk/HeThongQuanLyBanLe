FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ARG PROJECT

# Copy project files first so NuGet restore can be cached between code changes.
COPY ["BTL Web API/API_Admin.csproj", "BTL Web API/"]
COPY ["API_GateWay/API_GateWay.csproj", "API_GateWay/"]
COPY ["API_KeToan1/API_KeToan1.csproj", "API_KeToan1/"]
COPY ["API_Login/API_Login.csproj", "API_Login/"]
COPY ["API_ThuKho/API_ThuKho.csproj", "API_ThuKho/"]
COPY ["API_ThuNgan/API_ThuNgan.csproj", "API_ThuNgan/"]
COPY ["A/BLL.csproj", "A/"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["Model/Models.csproj", "Model/"]
RUN dotnet restore "$PROJECT"

COPY . .
RUN dotnet publish "$PROJECT" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ARG DLL
ENV APP_DLL=$DLL
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "exec dotnet \"$APP_DLL\""]