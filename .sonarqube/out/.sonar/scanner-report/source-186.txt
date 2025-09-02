FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /
COPY *.sln .
COPY . .
RUN dotnet restore -r linux-musl-x64 "RestaurantManagement.Api/RestaurantManagement.Api.csproj"

RUN dotnet publish -r linux-musl-x64 -c Release -o /app --no-restore --self-contained false --no-restore "RestaurantManagement.Api/RestaurantManagement.Api.csproj"

FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV TZ=Asia/Ho_Chi_Minh
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS="http://0.0.0.0:80/"
EXPOSE 80
ENTRYPOINT ["dotnet","RestaurantManagement.Api.dll"]
