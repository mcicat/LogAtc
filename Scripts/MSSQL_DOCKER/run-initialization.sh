# Wait to be sure that SQL Server came up
echo "Waiting until MSSQL server is up..."
echo "..."
sleep 30s

# Run the setup script to create the DB and the schema in the DB
# Note: make sure that your password matches what is in the Dockerfile
echo "Creating database..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Password123! -d master -i create-database.sql
echo "Database created"
