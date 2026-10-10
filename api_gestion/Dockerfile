<<<<<<< HEAD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore ApiGestion.csproj
RUN dotnet publish ApiGestion.csproj -c Release -o /app/publish /p:AssemblyName=ApiGestion

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8074
EXPOSE 8074
ENTRYPOINT ["dotnet", "ApiGestion.dll"]
=======
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
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10
