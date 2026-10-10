FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app
COPY . .

ENV DOTNET_USE_POLLING_FILE_WATCHER=true \
    ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_URLS=http://0.0.0.0:8010

EXPOSE 8010

CMD ["dotnet", "watch", "run", "--no-launch-profile"]
