SELECT 'CREATE DATABASE inventory_db'
WHERE NOT EXISTS (
    SELECT FROM pg_database WHERE datname = 'inventory_db'
)\gexec

SELECT 'CREATE DATABASE billing_db'
WHERE NOT EXISTS (
    SELECT FROM pg_database WHERE datname = 'billing_db'
)\gexec
