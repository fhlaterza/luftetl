using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using Dapper;
using luftetl.elements;
using Npgsql;
using DotNetEnv;
using System.Linq.Expressions;

class Program
{ 
        static void Main(string[] args)
         {
        
                // Carregar variáveis do arquivo .env

                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory) // Base do diretório do projeto
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                var executionIntervalMinutes = int.Parse(configuration["AppSettings:ExecutionIntervalMinutes"]);
                var periodSelection = int.Parse(configuration["AppSettings:PeriodSelection"]);

                Env.Load();
                Console.WriteLine($"SQLSERVER_HOST: {Environment.GetEnvironmentVariable("SQLSERVER_HOST")}");
                Console.WriteLine($"POSTGRES_HOST: {Environment.GetEnvironmentVariable("POSTGRES_HOST")}");

                // Construir strings de conexão usando variáveis de ambiente
                var sqlConnectionString = $"Server={Environment.GetEnvironmentVariable("SQLSERVER_HOST")};" +
                                        $"Database={Environment.GetEnvironmentVariable("SQLSERVER_DATABASE")};" +
                                        $"User Id={Environment.GetEnvironmentVariable("SQLSERVER_USER")};" +
                                        $"Password={Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD")};";

                var postgresConnectionString = $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST")};" +
                                            $"Database={Environment.GetEnvironmentVariable("POSTGRES_DATABASE")};" +
                                            $"Username={Environment.GetEnvironmentVariable("POSTGRES_USER")};" +
                                            $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")};";

                // // Obter strings de conexão
                // var sqlConnectionString = configuration.GetConnectionString("SqlServer");
                // var postgresConnectionString = configuration.GetConnectionString("PostgreSql");
                while (true)
                { 
                    var startTime = DateTime.Now;
                    LogExecutionTime("Início", startTime);
                    try 
                    {
                        // 1. Extração: Ler dados da base SQL
                        var data = ExtractData(sqlConnectionString, periodSelection);

                        // 2. Transformação: Ajustar os dados (se necessário)
                        var transformedData = TransformData(data);

                        // 3. Carregamento: Gravar os dados no PostgreSQL
                        LoadData(postgresConnectionString, transformedData, periodSelection);

                        var endTime = DateTime.Now;
                        LogExecutionTime("Término", endTime);
                        Console.WriteLine($"Execução concluída às {endTime}");

                    } catch (Exception ex) {
                        Console.WriteLine($"Erro durante a execução: {ex.Message}");
                        LogExecutionTime("Erro", DateTime.Now, ex.Message);
                    }

                    Console.WriteLine($"Aguardando {executionIntervalMinutes} minutos para a próxima execução...");
                    Thread.Sleep(TimeSpan.FromMinutes(executionIntervalMinutes));
                }   
        }
   static IEnumerable<Movimento> ExtractData(string connectionString, int periodSelection)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM view_Etl_movimentos where emissao > getdate() - {periodSelection} "; // Ajuste conforme necessário
                return connection.Query<Movimento>(query);
            }
        }

    static IEnumerable<Movimento> TransformData(IEnumerable<Movimento> data)
        {
            var transformed = new List<Movimento>();

            foreach (var item in data)
            {
                transformed.Add(new Movimento
                {
                    Id_Movimento = item.Id_Movimento,
                    Nr_Conhecimento = item.Nr_Conhecimento,   
                    Ds_TipoMovimento = item.Ds_TipoMovimento,
                    Luft = item.Luft,
                    Negocio = item.Negocio,
                    Id_Pessoa = item.Id_Pessoa,
                    Cd_CGCCPF = item.Cd_CGCCPF,
                    Ds_Cliente = item.Ds_Cliente,   //.ToUpper(), // Exemplo de transformação
                    CidFilial = item.CidFilial,
                    Emissao = item.Emissao,
                    Faturado = item.Faturado,
                    CnpjFaturado = item.CnpjFaturado,
                    RaizFaturado = item.RaizFaturado,
                    Ds_Cidade = item.Ds_Cidade,
                    Ds_Estado = item.Ds_Estado,
                    Entrega = item.Entrega,
                    PesoCubado = item.PesoCubado,
                    PesoReal = item.PesoReal,
                    VlrMercadoria = item.VlrMercadoria,
                    Vl_Frete = item.Vl_Frete,
                    Vl_IssIcms = item.Vl_IssIcms,
                    Vl_PisCofins = item.Vl_PisCofins,
                    Vl_Liquido = item.Vl_Liquido,
                    Aliquota = item.Aliquota,
                    Id_ManifestoEntrega = item.Id_ManifestoEntrega,
                    Id_ManifestoViagem = item.Id_ManifestoViagem,
                    FilialAtual = item.FilialAtual,
                    Status_Mov = item.Status_Mov,
                    Regional = item.Regional
                });
            }

            return transformed;
        }

    static void LoadData(string connectionString, IEnumerable<Movimento> data, int periodSelection)
        {
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string deleteQuery = $"DELETE FROM etlmovimentosC WHERE emissao > CURRENT_DATE - {periodSelection};";
                connection.Execute(deleteQuery);

                string insertQuery = @"
                    INSERT INTO etlmovimentosC (
                        id_movimento, nr_Conhecimento, ds_TipoMovimento, Luft, Negocio, id_pessoa, cd_CGCCPF,
                        ds_Cliente, cidfilial, Emissao, faturado, cnpjfaturado, raizfaturado, ds_Cidade,
                        ds_Estado, Entrega, pesocubado, pesoreal, VlrMercadoria, vl_frete, vl_issicms, 
                        vl_piscofins, vl_liquido, aliquota, id_ManifestoEntrega, id_ManifestoViagem, 
                        filialatual, STATUS_MOV, regional
                    ) VALUES (
                        @Id_Movimento, @Nr_Conhecimento, @Ds_TipoMovimento, @Luft, @Negocio, @Id_Pessoa, @Cd_CGCCPF,
                        @Ds_Cliente, @CidFilial, @Emissao, @Faturado, @CnpjFaturado, @RaizFaturado, @Ds_Cidade,
                        @Ds_Estado, @Entrega, @PesoCubado, @PesoReal, @VlrMercadoria, @Vl_Frete, @Vl_IssIcms, 
                        @Vl_PisCofins, @Vl_Liquido, @Aliquota, @Id_ManifestoEntrega, @Id_ManifestoViagem, 
                        @FilialAtual, @Status_Mov, @Regional
                    )";

                foreach (var item in data)
                {
                    connection.Execute(insertQuery, item);
                }
            }
        }
    static void LogExecutionTime(string eventType, DateTime time, string errorMessage = null)
    {
        string logFilePath = Path.Combine(AppContext.BaseDirectory, "execution_log.txt");
        string logMessage = $"{eventType}: {time:yyyy-MM-dd HH:mm:ss}";

        if (!string.IsNullOrEmpty(errorMessage))
        {
            logMessage += $" | Erro: {errorMessage}";
        }

        Console.WriteLine(logMessage);

        File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
    }
}

