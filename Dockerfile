FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

COPY *.sln .
COPY . .
WORKDIR /app/RestaurantManagement.Api
RUN dotnet restore RestaurantManagement.Api.csproj

RUN dotnet public RestaurantManagement.Api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY --from=build /app/RestaurantManagement.Api/out ./

ENV ASPNETCORE_URLS=http://0.0.0.0:8000
EXPOSE 8000

ENTRYPOINT [ "dotnet", "RestaurantManagement.Api.dll" ]
