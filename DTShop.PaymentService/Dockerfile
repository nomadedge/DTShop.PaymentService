FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["DTShop.PaymentService/DTShop.PaymentService.csproj", "DTShop.PaymentService/"]
COPY ["DTShop.PaymentService.Core/DTShop.PaymentService.Core.csproj", "DTShop.PaymentService.Core/"]
RUN dotnet restore "DTShop.PaymentService/DTShop.PaymentService.csproj"
COPY . .
WORKDIR "/src/DTShop.PaymentService"
RUN dotnet build "DTShop.PaymentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DTShop.PaymentService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DTShop.PaymentService.dll"]