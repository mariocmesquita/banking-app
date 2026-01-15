FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "./"]

COPY ["src/Services/CheckingAccountService/BankingApp.CheckingAccountService.Api/BankingApp.CheckingAccountService.Api.csproj", "src/Services/CheckingAccountService/BankingApp.CheckingAccountService.Api/"]
COPY ["src/Services/CheckingAccountService/BankingApp.CheckingAccountService.Application/BankingApp.CheckingAccountService.Application.csproj", "src/Services/CheckingAccountService/BankingApp.CheckingAccountService.Application/"]
COPY ["src/Services/CheckingAccountService/BankingApp.CheckingAccountService.Infrastructure/BankingApp.CheckingAccountService.Infrastructure.csproj", "src/Services/CheckingAccountService/BankingApp.CheckingAccountService.Infrastructure/"]
COPY ["src/Services/CheckingAccountService/BankingApp.CheckingAccountService.Domain/BankingApp.CheckingAccountService.Domain.csproj", "src/Services/CheckingAccountService/BankingApp.CheckingAccountService.Domain/"]
COPY ["src/Shared/BankingApp.Shared/BankingApp.Shared.csproj", "src/Shared/BankingApp.Shared/"]

RUN dotnet restore "src/Services/CheckingAccountService/BankingApp.CheckingAccountService.Api/BankingApp.CheckingAccountService.Api.csproj"

COPY . .

WORKDIR "/src/src/Services/CheckingAccountService/BankingApp.CheckingAccountService.Api"
RUN dotnet build -c Release -o /app/build

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN mkdir -p /app/data

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BankingApp.CheckingAccountService.Api.dll"]
