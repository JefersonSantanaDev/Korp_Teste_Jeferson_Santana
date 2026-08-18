# Build context: services/billing/

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Billing.Api/Billing.Api.csproj src/Billing.Api/
RUN dotnet restore src/Billing.Api/Billing.Api.csproj

COPY src/Billing.Api/ src/Billing.Api/
RUN dotnet publish src/Billing.Api/Billing.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Billing.Api.dll"]
