#!/bin/bash

echo "Iniciando configuración de la base de datos..."

# 1. Crea la base de datos 'gestion_local' si no existe
/opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'gestion_local') BEGIN CREATE DATABASE gestion_local; END"

# 2. Ejecuta tu archivo .sql para crear las tablas y sembrar los datos
/opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -d gestion_local -C -i /scripts/gestion_profesoral.sql

echo "Inicialización completada con éxito."