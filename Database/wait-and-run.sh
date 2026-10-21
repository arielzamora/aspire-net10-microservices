#!/bin/bash

# Use environment variable or default
SA_PASSWORD="${SA_PASSWORD:-Vaxi\$2025USPass}"
DB_SERVER="${DB_SERVER:-database}"

echo "Waiting for SQL Server at ${DB_SERVER} to be ready..."

for i in {1..50};
do
    /opt/mssql-tools18/bin/sqlcmd -C -S "${DB_SERVER}" -U sa -P "${SA_PASSWORD}" -Q "SELECT 1" > /dev/null 2>&1
    if [ $? -eq 0 ]
    then
        echo "SQL Server is ready. Executing seed script..."
        /opt/mssql-tools18/bin/sqlcmd -C -S "${DB_SERVER}" -U sa -P "${SA_PASSWORD}" -d master -i /CreateDatabaseAndSeed.sql
        
        if [ $? -eq 0 ]
        then
            echo "Database seeded successfully!"
            exit 0
        else
            echo "Error seeding database!"
            exit 1
        fi
    else
        echo "Attempt $i: SQL Server not ready yet..."
        sleep 2
    fi
done

echo "Timeout waiting for SQL Server!"
exit 1
