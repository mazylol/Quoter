FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Quoter/Quoter.csproj", "Quoter/"]
RUN dotnet restore "Quoter/Quoter.csproj"
COPY . .
WORKDIR "/src/Quoter"
RUN dotnet build "Quoter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quoter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY [".env", "/app"]
ENTRYPOINT ["dotnet", "Quoter.dll"]
