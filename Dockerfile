# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el proyecto y restaurar dependencias
COPY Balance.API.csproj .
RUN dotnet restore

# Copiar el resto del código y publicar la aplicación
COPY . .
RUN dotnet publish Balance.API.csproj -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Puerto que usará la aplicación
EXPOSE 80
EXPOSE 443

# Comando de inicio (entrypoint)
ENTRYPOINT ["dotnet", "Balance.API.dll"]