dotnet dev-certs https --trust
sqlcmd -S localhost\SQLEXPRESS -E -i "CREATE DATABASE.sql"
sqlcmd -S localhost\SQLEXPRESS -E -i "DATABASE SCRIPT.sql"
pause