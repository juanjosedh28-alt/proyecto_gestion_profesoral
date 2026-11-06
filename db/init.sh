#!/bin/bash
# ============================================================
# Prepara la base de datos gestion_local. Lo ejecuta el contenedor
# "sqlserver-init" cada vez que se hace docker compose up.
#
#   1. Crea la base si no existe.
#   2. SOLO LA PRIMERA VEZ crea las 19 tablas y siembra los datos de
#      referencia (gestion_profesoral.sql). Ese script BORRA y vuelve a crear
#      las tablas, así que correrlo en cada arranque perdería los datos.
#   3. SIEMPRE aplica v2_procedimientos.sql (vistas, procedimientos y
#      disparadores). Es idempotente: se puede correr las veces que sea.
#
# Opciones de sqlcmd:
#   -b        si el script falla, sqlcmd termina con error y el contenedor
#             queda en "Exited (1)" en vez de esconder el problema
#   -I        activa QUOTED_IDENTIFIER (recomendado para vistas y disparadores)
#   -f 65001  lee los archivos como UTF-8, para que las tildes lleguen bien
# ============================================================
set -e

# Atajo: sqlcmd ya conectado al servidor (la contraseña llega por variable de entorno)
ejecutar_sql() {
    /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P "$MSSQL_SA_PASSWORD" -C -b -I -f 65001 "$@"
}

echo "Iniciando configuración de la base de datos..."

# 1. La base
ejecutar_sql -Q "IF DB_ID('gestion_local') IS NULL CREATE DATABASE gestion_local;"

# 2. Las tablas (solo si todavía no existen)
YA_EXISTE=$(ejecutar_sql -d gestion_local -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN OBJECT_ID('dbo.docente', 'U') IS NULL THEN 0 ELSE 1 END;" | tr -d '[:space:]')
if [ "$YA_EXISTE" = "0" ]; then
    echo "Primera vez: creando tablas y datos de referencia (v1)..."
    ejecutar_sql -d gestion_local -i /scripts/gestion_profesoral.sql
else
    echo "Las tablas ya existen: se conservan los datos."
fi

# 3. Procedimientos, vistas y disparadores (v2)
echo "Aplicando procedimientos almacenados y disparadores (v2)..."
ejecutar_sql -d gestion_local -i /scripts/v2_procedimientos.sql

echo "Inicialización completada con éxito."
