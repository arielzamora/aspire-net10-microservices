#!/bin/sh
# Escribe la configuración del cliente con la URL de la API recibida por variable de entorno.
set -e

if [ -z "$API_URL" ]; then
  echo "Falta la variable de entorno API_URL" >&2
  exit 1
fi

cat > /usr/share/nginx/html/appsettings.json <<EOF
{
  "ApiOptions": {
    "WebApiBaseUrl": "${API_URL}"
  }
}
EOF
