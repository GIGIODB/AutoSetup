declare @hostname varchar(50) = '%'+@@servername+'%' select 1 from sys.certificates where name like @hostname

select @@servername , @hostname

declare @hostname varchar(50) = '%MS%' select 1 from sys.certificates where name like @hostname


create database gigio


SELECT name FROM sys.symmetric_keys

use master;
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'WYgfLzYwHsCBMCCGWsTqFZDdc70dpZeN'

USE gigio; 
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'WYgfLzYwHsCBMCCGWsTqFZDdc70dpZeN'

CREATE CERTIFICATE cert_tde_GIGIOMACHINE WITH SUBJECT = 'cert_tde_GIGIOMACHINE'

OPEN MASTER KEY DECRYPTION BY PASSWORD = 'WYgfLzYwHsCBMCCGWsTqFZDdc70dpZeN'  


use gigio 
drop certificate cert_tde_GIGIOMACHINE
DROP MASTER KEY 

use master
drop certificate cert_tde_GIGIOMACHINE
DROP MASTER KEY 

select * from sys.certificates

declare @cmd varchar(500) = 'CREATE CERTIFICATE [cert_tde_' + @@servername + ']'

exec (@cmd)