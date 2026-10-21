# Cliente Blazor WebAssembly: se publica como archivos estáticos y los sirve nginx.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY MasterNet.Client.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /out --no-restore

FROM nginx:alpine
COPY --from=build /out/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
# Los scripts de /docker-entrypoint.d se ejecutan al arrancar la imagen de nginx.
COPY docker-entrypoint.sh /docker-entrypoint.d/40-appsettings.sh
RUN chmod +x /docker-entrypoint.d/40-appsettings.sh
EXPOSE 80
