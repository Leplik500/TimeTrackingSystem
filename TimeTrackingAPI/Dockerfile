FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY TimeTrackingAPI/bin/Release/net8.0/publish/ .
ENTRYPOINT ["dotnet", "TimeTrackingAPI.dll"]