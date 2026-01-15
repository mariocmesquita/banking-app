FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "./"]

COPY ["src/ApiGateway/BankingApp.Gateway/BankingApp.Gateway.csproj", "src/ApiGateway/BankingApp.Gateway/"]

RUN dotnet restore "src/ApiGateway/BankingApp.Gateway/BankingApp.Gateway.csproj"

COPY . .

WORKDIR "/src/src/ApiGateway/BankingApp.Gateway"
RUN dotnet build -c Release -o /app/build

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BankingApp.Gateway.dll"]
