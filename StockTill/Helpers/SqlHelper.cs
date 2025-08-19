using ABI.System;
using Azure;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using DateTimeOffset = System.DateTimeOffset;

namespace StockTill.Helpers
{
    public sealed class SqlHelper
    {
        // 单例实例
        private static readonly Lazy<SqlHelper> _instance =
            new Lazy<SqlHelper>(() => new SqlHelper());

        public static SqlHelper Instance => _instance.Value;

        private const string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=StockTillDB;Trusted_Connection=True;TrustServerCertificate=True;";

        /*public bool Connect()
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            int count = (int)(new SqlCommand("SELECT COUNT(*) FROM sys.tables WHERE name = 'Goods' AND schema_id = SCHEMA_ID('StockTillSchema')", conn)).ExecuteScalar();
            return count > 0;
        }*/
        public async Task<bool> ConnectAsync()
        {
            string sql = "SELECT COUNT(*) FROM sys.tables WHERE name = 'Goods' AND schema_id = SCHEMA_ID('StockTillSchema')";
            var result = await ExecuteScalarAsync(sql);
            return result is int count && count > 0;
        }
        public void Initialize()
        {
            string initSql = @"
-- 1. 创建 Schema（如果不存在）
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'StockTillSchema')
    EXEC('CREATE SCHEMA [StockTillSchema]');

-- 2. 创建 Goods 表（如果不存在）
IF NOT EXISTS (SELECT * FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'Goods' AND s.name = 'StockTillSchema')
BEGIN
    CREATE TABLE [StockTillSchema].[Goods] (
        [id] varchar(255) NOT NULL,
        [name] varchar(255) COLLATE Chinese_PRC_CI_AS NULL,
        [quantity] int NOT NULL,
        [cost] decimal(18,2) NOT NULL,
        [price] decimal(18,2) NOT NULL,
        PRIMARY KEY CLUSTERED ([id])
    );
END

-- 3. 添加 Goods 表的描述（如果尚未设置）
IF NOT EXISTS (SELECT * FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'id'))
BEGIN
    EXEC sp_addextendedproperty N'MS_Description', N'商品编号',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'id';
    EXEC sp_addextendedproperty N'MS_Description', N'商品名称',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'name';
    EXEC sp_addextendedproperty N'MS_Description', N'库存数量',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'quantity';
    EXEC sp_addextendedproperty N'MS_Description', N'单件成本',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'cost';
    EXEC sp_addextendedproperty N'MS_Description', N'单件售价',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'price';
END

-- 4. 创建 OperationLog 表（如果不存在）
IF NOT EXISTS (SELECT * FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'OperationLog' AND s.name = 'StockTillSchema')
BEGIN
    CREATE TABLE [StockTillSchema].[OperationLog] (
        [id] varchar(255) NOT NULL,
        [operation] bit NOT NULL,
        [quantity] int NOT NULL,
        [time] datetimeoffset(7) NOT NULL
    );
END

-- 5. 添加 OperationLog 表的描述（如果尚未设置）
IF NOT EXISTS (SELECT * FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'StockTillSchema', N'TABLE', N'OperationLog', N'COLUMN', N'id'))
BEGIN
    EXEC sp_addextendedproperty N'MS_Description', N'商品编码',               N'SCHEMA', N'StockTillSchema', N'TABLE', N'OperationLog', N'COLUMN', N'id';
    EXEC sp_addextendedproperty N'MS_Description', N'0为入库，1为出库',       N'SCHEMA', N'StockTillSchema', N'TABLE', N'OperationLog', N'COLUMN', N'operation';
    EXEC sp_addextendedproperty N'MS_Description', N'数量',                   N'SCHEMA', N'StockTillSchema', N'TABLE', N'OperationLog', N'COLUMN', N'quantity';
    EXEC sp_addextendedproperty N'MS_Description', N'操作时间',               N'SCHEMA', N'StockTillSchema', N'TABLE', N'OperationLog', N'COLUMN', N'time';
END

-- 6. 设置 LOCK_ESCALATION（仅当表存在时）
IF EXISTS (SELECT * FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'Goods' AND s.name = 'StockTillSchema')
BEGIN
    ALTER TABLE [StockTillSchema].[Goods] SET (LOCK_ESCALATION = TABLE);
END
";

            ExecuteNonQuery(initSql);
        }
        public DataTable SelectAll()
        {
            string sql = "SELECT * FROM StockTillSchema.Goods";
            return ExecuteQuery(sql);
        }
        public DataRow? SelectById(string id)
        {
            string sql = "SELECT * FROM StockTillSchema.Goods WHERE id = @id";
            var dataTable = ExecuteQuery(sql, new SqlParameter("@id", id));
            return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
        }
        public void Insert(string id, string name, int quantity, decimal cost, decimal price)
        {
            string sql = """
                INSERT INTO StockTillSchema.Goods (id, name, quantity, cost, price)
                VALUES (@id, @name, @quantity, @cost, @price)
                """;
            ExecuteNonQuery(sql, [
                new SqlParameter("@id", id),
                new SqlParameter("@name", name),
                new SqlParameter("@quantity", quantity),
                new SqlParameter("@cost", cost),
                new SqlParameter("@price", price)
            ]);
        }
        public void UpdateById(string id, string name, int quantity, decimal cost, decimal price)
        {
            string sql = """
                UPDATE StockTillSchema.Goods 
                SET name = @name, quantity = @quantity, cost = @cost, price = @price 
                WHERE id = @id
                """;
            ExecuteNonQuery(sql, [
                new SqlParameter("@id", id),
                new SqlParameter("@name", name),
                new SqlParameter("@quantity", quantity),
                new SqlParameter("@cost", cost),
                new SqlParameter("@price", price)
            ]);
        }
        public void DeleteById(string id)
        {
            string sql = "DELETE FROM [StockTillSchema].[Goods] WHERE [id] = @id";
            ExecuteNonQuery(sql, new SqlParameter("@id", id));
        }
        public void ReduceQuantityById(string id, int reduce)
        {
            string sql = """
                UPDATE StockTillSchema.Goods 
                SET quantity = quantity - @reduce 
                WHERE id = @id
                """;
            ExecuteNonQuery(sql, [
                new SqlParameter("@reduce", reduce),
                new SqlParameter("@id", id)
            ]);
        }
        public void InsertLog(string id, bool isTill, int quantity)
        {
            string sql = """
                INSERT INTO StockTillSchema.OperationLog (id, operation, quantity, time)
                VALUES (@id, @operation, @quantity, @time)
                """;
            ExecuteNonQuery(sql, [
                new SqlParameter("@id", id),
                new SqlParameter("@operation", isTill),
                new SqlParameter("@quantity", quantity),
                new SqlParameter("@time", SqlDbType.DateTimeOffset) { Value = DateTimeOffset.Now }
            ]);
        }
        public DataTable SelectLog(bool? isTill, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            string sql = """
                SELECT
                  OperationLog.id,
                  name,
                  CASE
                    WHEN operation = 1 THEN
                      '出库'
                    WHEN operation = 0 THEN
                      '入库'
                  END AS operation,
                  OperationLog.quantity,
                  CASE
                    WHEN operation = 1 THEN
                      + price
                    WHEN operation = 0 THEN
                      - cost
                  END AS amount,
                  OperationLog.time
                FROM
                  StockTillSchema.OperationLog
                  INNER JOIN StockTillSchema.Goods ON OperationLog.id = Goods.id
                WHERE
                  (@isTill IS NULL OR operation = @isTill)
                  AND (TIME >= @startDate AND TIME <= @endDate)
                """;
            return ExecuteQuery(sql, [
                new SqlParameter("@isTill", SqlDbType.Bit) { Value = isTill ?? (object)DBNull.Value },
                new SqlParameter("@startDate", SqlDbType.DateTimeOffset) { Value = startDate },
                new SqlParameter("@endDate", SqlDbType.DateTimeOffset) { Value = endDate }
            ]);
        }
        public void DropSchema()
        {
            string sql = """
                DROP TABLE IF EXISTS [StockTillSchema].[Goods];
                DROP TABLE IF EXISTS [StockTillSchema].[OperationLog];
                DROP SCHEMA IF EXISTS [StockTillSchema];
                """;
            ExecuteNonQuery(sql);
        }
        private DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(ConnectionString);
            using var adapter = new SqlDataAdapter(sql, conn);

            if (parameters != null)
            {
                adapter.SelectCommand.Parameters.AddRange(parameters);
            }

            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            return dataTable; // 注意：Fill 完成后会自动打开/关闭连接
        }
        private int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();

            using var command = new SqlCommand(sql, conn);

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteNonQuery();
        }
        private bool ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();

            using var command = new SqlCommand(sql, conn);

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteScalar() != null;
        }
        private async Task<object?> ExecuteScalarAsync(string sql, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(sql, conn);

            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            await conn.OpenAsync();
            return await cmd.ExecuteScalarAsync();
        }
    }
}