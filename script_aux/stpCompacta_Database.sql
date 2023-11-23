ALTER PROCEDURE [dbo].[stpCompacta_Database] (
    @Ds_Database SYSNAME,
    @Fl_Rodar_Shrink BIT = 1,
    @Fl_Parar_Se_Falhar BIT = 1,
    @Fl_Exibe_Comparacao_Tamanho BIT = 1,
    @Fl_Metodo_Compressao_Page BIT = 1
)
AS
BEGIN
	/*
	Transact-SQL
	EXEC dbo.stpCompacta_Database
    @Ds_Database = 'Testes', -- sysname
    @Fl_Rodar_Shrink = 0, -- bit
    @Fl_Parar_Se_Falhar = 0, -- bit
    @Fl_Exibe_Comparacao_Tamanho = 1, -- bit
    @Fl_Metodo_Compressao_Page = 1 -- bit

	*/
 
    SET NOCOUNT ON
 
    
    DECLARE
        @Ds_Query VARCHAR(MAX),
        @Ds_Comando_Compactacao VARCHAR(MAX),
        @Ds_Metodo_Compressao VARCHAR(20) = (CASE WHEN @Fl_Metodo_Compressao_Page = 1 THEN 'PAGE' ELSE 'ROW' END),
        @Nr_Metodo_Compressao VARCHAR(20) = (CASE WHEN @Fl_Metodo_Compressao_Page = 1 THEN 2 ELSE 1 END)
        
        
    IF (OBJECT_ID('tempdb..#Comandos_Compactacao') IS NOT NULL) DROP TABLE #Comandos_Compactacao
    CREATE TABLE #Comandos_Compactacao
    (
        Id BIGINT IDENTITY(1, 1),
        Tabela SYSNAME,
        Indice SYSNAME NULL,
        Comando VARCHAR(MAX)
    )
    
    
    IF (@Fl_Exibe_Comparacao_Tamanho = 1)
    BEGIN
        
        SET @Ds_Query = '
        SELECT 
            (SUM(a.total_pages) / 128) AS Vl_Tamanho_Tabelas_Antes_Compactacao
        FROM 
            [' + @Ds_Database + '].sys.tables                    t     WITH(NOLOCK)
            INNER JOIN [' + @Ds_Database + '].sys.indexes            i     WITH(NOLOCK) ON t.OBJECT_ID = i.object_id
            INNER JOIN [' + @Ds_Database + '].sys.partitions            p     WITH(NOLOCK) ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
            INNER JOIN [' + @Ds_Database + '].sys.allocation_units        a     WITH(NOLOCK) ON p.partition_id = a.container_id
        WHERE 
            i.OBJECT_ID > 255 
			and t.name not in (''MonitoramentoCoex'')
        '
        
        EXEC(@Ds_Query)
        
    END
        
    SET @Ds_Query =
    'INSERT INTO #Comandos_Compactacao( Tabela, Indice, Comando )
    SELECT DISTINCT 
        A.name AS Tabela,
        NULL AS Indice,
        ''ALTER TABLE ['' + ''' + @Ds_Database + ''' + ''].['' + C.name + ''].['' + A.name + ''] REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = ' + @Ds_Metodo_Compressao + ')'' AS Comando
    FROM 
        [' + @Ds_Database + '].sys.tables                   A
        INNER JOIN [' + @Ds_Database + '].sys.partitions    B   ON A.object_id = B.object_id
        INNER JOIN [' + @Ds_Database + '].sys.schemas       C   ON A.schema_id = C.schema_id
    WHERE 
        B.data_compression <> ' + @Nr_Metodo_Compressao + ' -- NONE
        AND B.index_id = 0
        AND A.type = ''U''
		and A.name not in (''MonitoramentoCoex'')
    
    UNION
 
    SELECT DISTINCT 
        B.name AS Tabela,
        A.name AS Indice,
        ''ALTER INDEX ['' + A.name + ''] ON ['' + ''' + @Ds_Database + ''' + ''].['' + C.name + ''].['' + B.name + ''] REBUILD PARTITION = ALL WITH ( STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, SORT_IN_TEMPDB = OFF, DATA_COMPRESSION = ' + @Ds_Metodo_Compressao + ')''
    FROM 
        [' + @Ds_Database + '].sys.indexes                  A
        INNER JOIN [' + @Ds_Database + '].sys.tables        B   ON A.object_id = B.object_id
        INNER JOIN [' + @Ds_Database + '].sys.schemas       C   ON B.schema_id = C.schema_id
        INNER JOIN [' + @Ds_Database + '].sys.partitions    D   ON A.object_id = D.object_id AND A.index_id = D.index_id
    WHERE 
        D.data_compression <> ' + @Nr_Metodo_Compressao + ' -- NONE
        AND D.index_id <> 0
        AND B.type = ''U''
		and B.name not in (''MonitoramentoCoex'')
    ORDER BY
        Tabela,
        Indice
    '
    
    EXEC(@Ds_Query)
                        
        
    DECLARE 
        @Qt_Comandos INT = (SELECT COUNT(*) FROM #Comandos_Compactacao),
        @Contador INT = 1,
        @Ds_Mensagem VARCHAR(MAX),
        @Nr_Codigo_Erro INT = (CASE WHEN @Fl_Parar_Se_Falhar = 1 THEN 16 ELSE 10 END)
        
         
    WHILE(@Contador <= @Qt_Comandos)
    BEGIN
        
        SELECT
            @Ds_Comando_Compactacao = Comando
        FROM
            #Comandos_Compactacao
        WHERE
            Id = @Contador
                    
        BEGIN TRY
            
            SET @Ds_Mensagem = 'Executando comando "' + @Ds_Comando_Compactacao + '"... Aguarde...'
            print (@Ds_Mensagem)
			RAISERROR(@Ds_Mensagem, 10, 1) WITH NOWAIT 
            
			EXEC(@Ds_Comando_Compactacao)
             
        END TRY
                  
        BEGIN CATCH
            
            SELECT
                ERROR_NUMBER() AS ErrorNumber,
                ERROR_SEVERITY() AS ErrorSeverity,
                ERROR_STATE() AS ErrorState,
                ERROR_PROCEDURE() AS ErrorProcedure,
                ERROR_LINE() AS ErrorLine,
                ERROR_MESSAGE() AS ErrorMessage;
            
            SET @Ds_Mensagem = 'Falha ao executar o comando "' + @Ds_Comando_Compactacao + '"'
            RAISERROR(@Ds_Mensagem, @Nr_Codigo_Erro, 1) WITH NOWAIT
            
            RETURN
                        
        END CATCH    
        
        
        SET @Contador = @Contador + 1
        
    END
    
    
    
    IF (@Fl_Exibe_Comparacao_Tamanho = 1)
    BEGIN
        
        SET @Ds_Query = '
        SELECT 
            (SUM(a.total_pages) / 128) AS Vl_Tamanho_Tabelas_Depois_Compactacao
        FROM 
            [' + @Ds_Database + '].sys.tables                    t     WITH(NOLOCK)
            INNER JOIN [' + @Ds_Database + '].sys.indexes            i     WITH(NOLOCK) ON t.OBJECT_ID = i.object_id
            INNER JOIN [' + @Ds_Database + '].sys.partitions            p     WITH(NOLOCK) ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
            INNER JOIN [' + @Ds_Database + '].sys.allocation_units        a     WITH(NOLOCK) ON p.partition_id = a.container_id
        WHERE 
            i.OBJECT_ID > 255
			and t.name not in (''MonitoramentoCoex'')
        '
        
        EXEC(@Ds_Query)
        
    END
    
    
    IF (@Fl_Rodar_Shrink = 1)
    BEGIN
    
        SET @Ds_Query = '
        USE ' + @Ds_Database + '
        DBCC SHRINKFILE (' + @Ds_Database + ', 1) WITH NO_INFOMSGS
        '
        
        EXEC(@Ds_Query)
        
    END
    
    
    IF (@Qt_Comandos > 0)
        PRINT 'Database "' + @Ds_Database + '" compactado com sucesso!'
    ELSE
        PRINT 'Nenhum objeto para compactar no database "' + @Ds_Database + '"'
    
    
    
END