FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "./"]

COPY ["src/Services/FeeService/BankingApp.FeeService.Worker/BankingApp.FeeService.Worker.csproj", "src/Services/FeeService/BankingApp.FeeService.Worker/"]
COPY ["src/Services/FeeService/BankingApp.FeeService.Application/BankingApp.FeeService.Application.csproj", "src/Services/FeeService/BankingApp.FeeService.Application/"]
COPY ["src/Services/FeeService/BankingApp.FeeService.Infrastructure/BankingApp.FeeService.Infrastructure.csproj", "src/Services/FeeService/BankingApp.FeeService.Infrastructure/"]
COPY ["src/Services/FeeService/BankingApp.FeeService.Domain/BankingApp.FeeService.Domain.csproj", "src/Services/FeeService/BankingApp.FeeService.Domain/"]

RUN dotnet restore "src/Services/FeeService/BankingApp.FeeService.Worker/BankingApp.FeeService.Worker.csproj"

COPY . .

WORKDIR "/src/src/Services/FeeService/BankingApp.FeeService.Worker"
RUN dotnet build -c Release -o /app/build

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

RUN mkdir -p /app/data

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BankingApp.FeeService.Worker.dll"]
