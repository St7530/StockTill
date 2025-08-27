using Microsoft.Data.SqlClient;
using System.Data;
using System.Windows;
using DateTimeOffset = System.DateTimeOffset;

namespace StockTill.Helpers
{
    internal static class SqlHelper
    {
        private static string ConnectionString => ConfigHelper.GetConnectionString();

        public static async Task<bool> ConnectAsync()
        {
            string sql = "SELECT COUNT(*) FROM sys.tables WHERE name = 'Goods' AND schema_id = SCHEMA_ID('StockTillSchema')";
            var result = await ExecuteScalarAsync(sql);
            return result is int count && count > 0;
        }
        public static void Initialize()
        {
            string initSql = @"
-- 1. 创建 StockTillSchema 模式
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'StockTillSchema')
    EXEC('CREATE SCHEMA [StockTillSchema]');

-- 2. 创建 Goods 表
IF NOT EXISTS (SELECT * FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'Goods' AND s.name = 'StockTillSchema')
BEGIN
    CREATE TABLE [StockTillSchema].[Goods] (
        [id] varchar(255) NOT NULL,
        [name] varchar(255) COLLATE Chinese_PRC_CI_AS NULL,
        [category_id] int NULL,
        [quantity] int NOT NULL,
        [cost] decimal(18,2) NOT NULL,
        [price] decimal(18,2) NOT NULL,
        PRIMARY KEY CLUSTERED ([id])
    );
END

-- 3. 添加 Goods 表的描述
IF NOT EXISTS (SELECT * FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'id'))
BEGIN
    EXEC sp_addextendedproperty N'MS_Description', N'商品编号',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'id';
    EXEC sp_addextendedproperty N'MS_Description', N'商品名称',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'name';
    EXEC sp_addextendedproperty N'MS_Description', N'分类编号',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'category_id';
    EXEC sp_addextendedproperty N'MS_Description', N'库存数量',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'quantity';
    EXEC sp_addextendedproperty N'MS_Description', N'单件成本',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'cost';
    EXEC sp_addextendedproperty N'MS_Description', N'单件售价',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Goods', N'COLUMN', N'price';
END

-- 4. 创建 Categories 表
IF NOT EXISTS (SELECT * FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'Categories' AND s.name = 'StockTillSchema')
BEGIN
    CREATE TABLE [StockTillSchema].[Categories] (
      [category_id] int IDENTITY(1,1) NOT NULL,
      [category] varchar(255) COLLATE Chinese_PRC_CI_AS NULL,
      PRIMARY KEY CLUSTERED ([category_id])
    )
END

-- 5. 添加 Categories 表的描述
IF NOT EXISTS (SELECT * FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'StockTillSchema', N'TABLE', N'Categories', N'COLUMN', N'category_id'))
BEGIN
    EXEC sp_addextendedproperty N'MS_Description', N'分类编号',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Categories', N'COLUMN', N'category_id';
    EXEC sp_addextendedproperty N'MS_Description', N'商品分类',           N'SCHEMA', N'StockTillSchema', N'TABLE', N'Categories', N'COLUMN', N'category';
END

-- 6. 创建 OperationLog 表
IF NOT EXISTS (SELECT * FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'OperationLog' AND s.name = 'StockTillSchema')
BEGIN
    CREATE TABLE [StockTillSchema].[OperationLog] (
        [id] varchar(255) NOT NULL,
        [operation] bit NOT NULL,
        [quantity] int NOT NULL,
        [time] datetimeoffset(7) NOT NULL
    );
END

-- 7. 添加 OperationLog 表的描述
IF NOT EXISTS (SELECT * FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'StockTillSchema', N'TABLE', N'OperationLog', N'COLUMN', N'id'))
BEGIN
    EXEC sp_addextendedproperty N'MS_Description', N'商品编码',               N'SCHEMA', N'StockTillSchema', N'TABLE', N'OperationLog', N'COLUMN', N'id';
    EXEC sp_addextendedproperty N'MS_Description', N'0为入库，1为出库',       N'SCHEMA', N'StockTillSchema', N'TABLE', N'OperationLog', N'COLUMN', N'operation';
    EXEC sp_addextendedproperty N'MS_Description', N'数量',                   N'SCHEMA', N'StockTillSchema', N'TABLE', N'OperationLog', N'COLUMN', N'quantity';
    EXEC sp_addextendedproperty N'MS_Description', N'操作时间',               N'SCHEMA', N'StockTillSchema', N'TABLE', N'OperationLog', N'COLUMN', N'time';
END
";

            ExecuteNonQuery(initSql);
        }
        public static DataTable SelectAll()
        {
            string sql = """     
                SELECT
                  id,
                  name,
                  category,
                  quantity,
                  cost,
                  price
                FROM
                  StockTillSchema.Goods
                  INNER JOIN StockTillSchema.Categories ON Goods.category_id = Categories.category_id
                """;
            return ExecuteQuery(sql);
        }
        public static DataRow? SelectById(string id)
        {
            string sql = """     
                SELECT
                  id,
                  name,
                  category,
                  quantity,
                  cost,
                  price
                FROM
                  StockTillSchema.Goods
                  INNER JOIN StockTillSchema.Categories ON Goods.category_id = Categories.category_id
                WHERE
                  id = @id
                """;
            var dataTable = ExecuteQuery(sql, new SqlParameter("@id", id));
            return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
        }
        public static void Insert(string id, string name, int category_id, int quantity, decimal cost, decimal price)
        {
            string sql = """
                INSERT INTO StockTillSchema.Goods (id, name, category_id, quantity, cost, price)
                VALUES (@id, @name, @category_id, @quantity, @cost, @price)
                """;
            ExecuteNonQuery(sql, [
                new SqlParameter("@id", id),
                new SqlParameter("@name", name),
                new SqlParameter("@category_id", category_id),
                new SqlParameter("@quantity", quantity),
                new SqlParameter("@cost", cost),
                new SqlParameter("@price", price)
            ]);
        }
        public static void UpdateById(string id, string name, int category_id, int quantity, decimal cost, decimal price)
        {
            string sql = """
                UPDATE StockTillSchema.Goods 
                SET name = @name, category_id = @category_id, quantity = @quantity, cost = @cost, price = @price 
                WHERE id = @id
                """;
            ExecuteNonQuery(sql, [
                new SqlParameter("@id", id),
                new SqlParameter("@name", name),
                new SqlParameter("@category_id", category_id),
                new SqlParameter("@quantity", quantity),
                new SqlParameter("@cost", cost),
                new SqlParameter("@price", price)
            ]);
        }
        public static void DeleteById(string id)
        {
            string sql = "DELETE FROM [StockTillSchema].[Goods] WHERE [id] = @id";
            ExecuteNonQuery(sql, new SqlParameter("@id", id));
        }
        public static void ReduceQuantityById(string id, int reduce)
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
        public static DataTable SelectAllCategories()
        {
            string sql = "SELECT * FROM StockTillSchema.Categories";
            return ExecuteQuery(sql);
        }
        public static string? SelectCategoryById(int category_id)
        {
            string sql = "SELECT category FROM StockTillSchema.Categories WHERE category_id = @category_id";
            var dataTable = ExecuteQuery(sql, new SqlParameter("@category_id", category_id));
            return (string?)(dataTable.Rows.Count > 0 ? dataTable.Rows[0][0] : null);
        }
        public static int? SelectCategoryIdByCategory(string category)
        {
            string sql = "SELECT category_id FROM StockTillSchema.Categories WHERE category = @category";
            var dataTable = ExecuteQuery(sql, new SqlParameter("@category", category));
            return (int?)(dataTable.Rows.Count > 0 ? dataTable.Rows[0][0] : null);
        }
        public static void InsertCategory(string category)
        {
            string sql = """
                INSERT INTO StockTillSchema.Categories (category)
                VALUES (@category)
                """;
            ExecuteNonQuery(sql, new SqlParameter("@category", category));
        }
        public static void UpdateCategory(int category_id, string category)
        {
            string sql = """
                UPDATE StockTillSchema.Categories 
                SET category = @category
                WHERE category_id = @category_id
                """;
            ExecuteNonQuery(sql, [
                new SqlParameter("@category_id", category_id),
                new SqlParameter("@category", category)
            ]);
        }
        public static void DeleteCategoryById(int category_id)
        {
            string sql = "DELETE FROM [StockTillSchema].[Categories] WHERE [category_id] = @category_id";
            ExecuteNonQuery(sql, new SqlParameter("@category_id", category_id));
        }
        public static void InsertLog(string id, bool isTill, int quantity)
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
        public static DataTable SelectLog(bool? isTill, DateTimeOffset startDate, DateTimeOffset endDate)
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
        public static void DropSchema()
        {
            string sql = """
                DROP TABLE IF EXISTS [StockTillSchema].[Goods];
                DROP TABLE IF EXISTS [StockTillSchema].[Categories];
                DROP TABLE IF EXISTS [StockTillSchema].[OperationLog];
                DROP SCHEMA IF EXISTS [StockTillSchema];
                """;
            ExecuteNonQuery(sql);
        }
        private static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
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
        private static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
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
        private static bool ExecuteScalar(string sql, params SqlParameter[] parameters)
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
        private static async Task<object?> ExecuteScalarAsync(string sql, params SqlParameter[] parameters)
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