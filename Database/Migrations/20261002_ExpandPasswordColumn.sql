-- Run once if the existing PASS column is too short for the allowed password length.
-- Passwords are stored as plaintext by the current application behavior.
ALTER TABLE dbo.TAIKHOAN
    ALTER COLUMN PASS NVARCHAR(256) NOT NULL;
