-- Reset Database for API Tests
-- This script will delete all data from the tables and reset the auto-incrementing primary keys.

-- to start the script in Banking26DDDModules    
-- sqlite3 BankingApi/BankingDb.db <  BankingApi/_4_ApiTests/ResetTestDb.sql

-- Keeps the EF Core migration history table intact.

PRAGMA foreign_keys = OFF;

DELETE FROM "Beneficiaries";
DELETE FROM "Transactions";
DELETE FROM "Transfers";
DELETE FROM "Accounts";
DELETE FROM "Customers";
DELETE FROM "Employees";

PRAGMA foreign_keys = ON;
-- End of ResetTestDb.sql
       

       
