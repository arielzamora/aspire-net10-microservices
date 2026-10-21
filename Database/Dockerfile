FROM mcr.microsoft.com/mssql/server:2022-latest

COPY ./wait-and-run.sh /wait-and-run.sh
COPY ./CreateDatabaseAndSeed.sql /CreateDatabaseAndSeed.sql

# Ensure non-Windows line endings
USER root

RUN apt-get update && apt-get install -y dos2unix
RUN dos2unix /wait-and-run.sh
RUN dos2unix /CreateDatabaseAndSeed.sql
RUN chmod 777 /wait-and-run.sh

USER mssql

CMD /wait-and-run.sh
