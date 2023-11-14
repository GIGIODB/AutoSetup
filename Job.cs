using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupDatabase {
    public class Job {
        private string connectionString;
        public Job(string connectionString) {
            this.connectionString = connectionString;
        }

        public string CreateJobStats(string stringConexao) {
            string result;
            StringBuilder errorMessages = new StringBuilder();

            try {
                using (SqlConnection connection = new SqlConnection(stringConexao)) {
                    connection.Open();

                    string createJobStats =
                    $@"
                    USE [msdb];                                      

                    	/****** Object:  Job [DBA - UpdateStatisticsFull - autosetup]    Script Date: 11/13/2023 10:08:09 PM ******/
                    	BEGIN TRANSACTION
                    	DECLARE @ReturnCode INT
                    	SELECT @ReturnCode = 0
                    	/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 11/13/2023 10:08:09 PM ******/
                    	IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
                    	BEGIN
                    	EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
                    	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

                    	END

                    	DECLARE @jobId BINARY(16)
                    	EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'DBA - UpdateStatisticsFull - autosetup', 
                    			@enabled=1, 
                    			@notify_level_eventlog=0, 
                    			@notify_level_email=0, 
                    			@notify_level_netsend=0, 
                    			@notify_level_page=0, 
                    			@delete_level=0, 
                    			@description=N'No description available.', 
                    			@category_name=N'[Uncategorized (Local)]', 
                    			@owner_login_name=N'sa', @job_id = @jobId OUTPUT
                    	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
                    	/****** Object:  Step [Update statistics]    Script Date: 11/13/2023 10:08:09 PM ******/
                    	EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Update statistics', 
                    			@step_id=1, 
                    			@cmdexec_success_code=0, 
                    			@on_success_action=1, 
                    			@on_success_step_id=0, 
                    			@on_fail_action=2, 
                    			@on_fail_step_id=0, 
                    			@retry_attempts=0, 
                    			@retry_interval=0, 
                    			@os_run_priority=0, @subsystem=N'TSQL', 
                    			@command=N'
                    	declare c_lista insensitive cursor for   
                    		select 
                    			b.name 
                    		from sys.databases b   
                    		where b.database_id > 4 
                    		order by b.database_id asc

                    	declare @db sysname  
                    	open c_lista  
                    	fetch c_lista into @db  

                    	declare @sql nvarchar(4000)  
                    		while (@@fetch_status=0)  
                    		begin  
                    		set @sql = 

                    		''
                    			use ''+@db+'';

                    			SET NOCOUNT ON

                    			Create table #Update_Stats(
                    			Id_Stats int identity(1,1),
                    			Ds_Comand varchar(4000),
                    			Nr_Rows int)

                    			;WITH Size_Tables AS (
                    			SELECT obj.name, prt.rows
                    			FROM sys.objects obj
                    			JOIN sys.indexes idx on obj.object_id= idx.object_id
                    			JOIN sys.partitions prt on obj.object_id= prt.object_id
                    			JOIN sys.allocation_units alloc on alloc.container_id= prt.partition_id
                    			WHERE obj.type= ''''U'''' AND idx.index_id IN (0, 1)and prt.rows> 1000
                    			GROUP BY obj.name, prt.rows)

                    			insert into #Update_Stats(Ds_Comand,Nr_Rows)
                    			SELECT ''''UPDATE STATISTICS '''' + sc.name +''''.''''+ B.name + '''' '''' + quotename(A.name) + '''' WITH FULLSCAN, maxdop = 4'''', D.rows
                    			FROM sys.stats A
                    			join sys.objects B on A.object_id = B.object_id
                    			join sys.schemas sc on sc.schema_id = B.schema_id
                    			join sys.sysindexes C on C.id = B.object_id and A.name= C.Name
                    			OUTER APPLY sys.dm_db_stats_properties(B.object_id, A.stats_id) E
                    			JOIN Size_Tables D on B.name= D.name
                    			WHERE (C.rowmodctr > 100
                    			and C.rowmodctr> D.rows*.005
                    			or E.rows_sampled < (D.rows * 0.7))
                    			and sc.name not in (''''sys'''',''''dtp'''')
                    			and B.type = ''''U''''
                    			and B.name not in (''''t_documento_conteudo'''')
                    			ORDER BY D.rows

                    			declare @Loop int, @Comand nvarchar(4000)
                    			set @Loop = 1

                    			while exists(select top 1 null from #Update_Stats)
                    			begin

                    			select @Comand = Ds_Comand
                    			from #Update_Stats
                    			where Id_Stats = @Loop

                    			EXECUTE sp_executesql @Comand

                    			delete from #Update_Stats
                    			where Id_Stats = @Loop

                    			set @Loop= @Loop + 1
                    			end''	   	  
                    		exec sp_executesql @statement = @sql
                    		--print(@sql)
                    		fetch c_lista into @db  
                    		end  
                    	deallocate c_lista', 
                    			@database_name=N'master', 
                    			@flags=0
                    	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
                    	EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
                    	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
                    	EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'2 da manha', 
                    			@enabled=1, 
                    			@freq_type=4, 
                    			@freq_interval=1, 
                    			@freq_subday_type=1, 
                    			@freq_subday_interval=0, 
                    			@freq_relative_interval=0, 
                    			@freq_recurrence_factor=0, 
                    			@active_start_date=20230609, 
                    			@active_end_date=99991231, 
                    			@active_start_time=20000, 
                    			@active_end_time=235959, 
                    			@schedule_uid=N'1a8cf634-1145-4e46-bb28-03f776dfe14c'
                    	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
                    	EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
                    	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
                    	COMMIT TRANSACTION
                    	GOTO EndSave
                    	QuitWithRollback:
                    	    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
                    	EndSave:
                        ";

                    using (SqlCommand command = new SqlCommand(createJobStats, connection)) {
                        try {
                            command.ExecuteNonQuery();
                        }
                        catch (SqlException ex) {

                            for (int i = 0; i < ex.Errors.Count; i++) {
                                errorMessages.Append("Index #" + i + "\n" +
                                    "Message: " + ex.Errors[i].Message + "\n" +
                                    "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                                    "Source: " + ex.Errors[i].Source + "\n");
                            }
                            Console.WriteLine(errorMessages.ToString());
                        }
                    }
                }
                result = $"\n\n Job [DBA - UpdateStatisticsFull - autosetup] criado com sucesso! Por default o agendamento esta às 02 da manhã.";
                Console.WriteLine(result);
                Console.ReadLine();
                return result;
            }
            catch (SqlException ex) {

                for (int i = 0; i < ex.Errors.Count; i++) {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
                result = errorMessages.ToString();
                Console.ReadLine();
                return result;
            }
        }
    }
}
