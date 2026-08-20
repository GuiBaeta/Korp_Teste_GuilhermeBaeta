FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Billing.Api.csproj ./
RUN dotnet restore Billing.Api.csproj

COPY . ./
RUN dotnet publish Billing.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Billing.Api.dll"]
