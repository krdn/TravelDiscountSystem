FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TravelDiscountSystem.csproj", "."]
RUN dotnet restore "TravelDiscountSystem.csproj"
COPY . .
RUN dotnet build "TravelDiscountSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TravelDiscountSystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TravelDiscountSystem.dll"]