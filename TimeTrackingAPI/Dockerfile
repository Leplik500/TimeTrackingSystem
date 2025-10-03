# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY TimeTrackingAPI/TimeTrackingAPI.csproj TimeTrackingAPI/
RUN dotnet restore TimeTrackingAPI/TimeTrackingAPI.csproj

# Копируем весь код и собираем приложение
COPY . .
RUN dotnet build TimeTrackingAPI/TimeTrackingAPI.csproj -c Release -o /app/build
RUN dotnet publish TimeTrackingAPI/TimeTrackingAPI.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Копируем только скомпилированные файлы из build stage
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TimeTrackingAPI.dll"]
