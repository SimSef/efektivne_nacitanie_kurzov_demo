FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["NacitanieKurzovConsoleApp.csproj", "./"]

RUN dotnet restore "NacitanieKurzovConsoleApp.csproj"

COPY . .

RUN dotnet publish "NacitanieKurzovConsoleApp.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NacitanieKurzovConsoleApp.dll"]

