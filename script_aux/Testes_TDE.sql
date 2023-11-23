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
--FRPYrMa2EaqRjFGtY37zYyBuJbaGjNRm
--C:\temp\certificadotde\cert_tde_GIGIOMACHINE.cer
--C:\temp\certificadotde\cert_tde_GIGIOMACHINE.key
select * from sys.certificates

SELECT
	            A.[name] as name,
	            isnull(C.name,'No certificate') as certificate,	            
	            isnull(A.is_encrypted,0) as is_encrypted
            FROM sys.databases A
	        LEFT JOIN sys.dm_database_encryption_keys B ON B.database_id = A.database_id
	        LEFT JOIN sys.certificates c on c.thumbprint = b.encryptor_thumbprint
            where A.database_id > 4
            and isnull(C.name,'No certificate') not like 'cert_tde_{hostname}';

/*		Verificar o status da encrypted  
	1 = sem crypto  
	2 = mudando de status 1/3 
	3 = crypto  
	5 = mudando de status 3/1 
	cert_tde_GIGIOMACHINE */
	SELECT
		@@SERVERNAME,
		A.[name],
		A.is_master_key_encrypted_by_server,
		A.is_encrypted,
		B.percent_complete,
		B.*
	FROM
		sys.databases A
		JOIN sys.dm_database_encryption_keys B ON B.database_id = A.database_id
	where a.name in('gigio')

	alter database gigio set encryption off

	drop DATABASE ENCRYPTION KEY
	GO

	sp_help 'sys.dm_database_encryption_keys'
--use gigio
--create table dados 
--(
-- name varchar(70)
--)
--
insert into dados
select name
from dados
--
--sp_spaceused dados

