FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "./"]

COPY ["src/Services/TransferService/BankingApp.TransferService.Api/BankingApp.TransferService.Api.csproj", "src/Services/TransferService/BankingApp.TransferService.Api/"]
COPY ["src/Services/TransferService/BankingApp.TransferService.Application/BankingApp.TransferService.Application.csproj", "src/Services/TransferService/BankingApp.TransferService.Application/"]
COPY ["src/Services/TransferService/BankingApp.TransferService.Infrastructure/BankingApp.TransferService.Infrastructure.csproj", "src/Services/TransferService/BankingApp.TransferService.Infrastructure/"]
COPY ["src/Services/TransferService/BankingApp.TransferService.Domain/BankingApp.TransferService.Domain.csproj", "src/Services/TransferService/BankingApp.TransferService.Domain/"]

RUN dotnet restore "src/Services/TransferService/BankingApp.TransferService.Api/BankingApp.TransferService.Api.csproj"

COPY . .

WORKDIR "/src/src/Services/TransferService/BankingApp.TransferService.Api"
RUN dotnet build -c Release -o /app/build

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN mkdir -p /app/data

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BankingApp.TransferService.Api.dll"]
