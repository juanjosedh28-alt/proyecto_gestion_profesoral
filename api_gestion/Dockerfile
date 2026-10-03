FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app

# Copiamos los archivos de la API
COPY . .

# Configuramos el puerto y el entorno para que se actualice en tiempo real (watch)
ENV DOTNET_USE_POLLING_FILE_WATCHER=true \
    ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_URLS=http://0.0.0.0:8074

EXPOSE 8074

CMD ["dotnet", "watch", "run", "--no-launch-profile"]