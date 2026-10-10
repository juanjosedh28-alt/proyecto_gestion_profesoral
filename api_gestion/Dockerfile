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