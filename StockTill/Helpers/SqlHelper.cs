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

        private const string ConnectionString ="Server=localhost\\SQLEXPRESS;Database=StockTillDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public bool Connect()
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            int count = (int)(new SqlCommand("SELECT COUNT(*) FROM sys.tables WHERE name = 'Goods' AND schema_id = SCHEMA_ID('StockTillSchema')", conn)).ExecuteScalar();
            return count > 0;
        }

        public void Initialize()
        {
            string[] initSqls = { @"
CREATE TABLE [StockTillSchema].[Goods] (
  [id] varchar(255)  NOT NULL,
  [name] varchar(255) COLLATE Chinese_PRC_CI_AS  NULL,
  [quantity] int  NOT NULL,
  [cost] decimal(18,2)  NOT NULL,
  [price] decimal(18,2)  NOT NULL,
  PRIMARY KEY CLUSTERED ([id])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
)  
ON [PRIMARY]", @"
ALTER TABLE [StockTillSchema].[Goods] SET (LOCK_ESCALATION = TABLE)", @"
EXEC sp_addextendedproperty
'MS_Description', N'商品编号',
'SCHEMA', N'StockTillSchema',
'TABLE', N'Goods',
'COLUMN', N'id'", @"
EXEC sp_addextendedproperty
'MS_Description', N'商品名称',
'SCHEMA', N'StockTillSchema',
'TABLE', N'Goods',
'COLUMN', N'name'", @"
EXEC sp_addextendedproperty
'MS_Description', N'库存数量',
'SCHEMA', N'StockTillSchema',
'TABLE', N'Goods',
'COLUMN', N'quantity'", @"
EXEC sp_addextendedproperty
'MS_Description', N'单件成本',
'SCHEMA', N'StockTillSchema',
'TABLE', N'Goods',
'COLUMN', N'cost'", @"
EXEC sp_addextendedproperty
'MS_Description', N'单价售价',
'SCHEMA', N'StockTillSchema',
'TABLE', N'Goods',
'COLUMN', N'price'", @"
CREATE TABLE [StockTillSchema].[OperationLog] (
  [id] varchar(255) NOT NULL,
  [operation] bit NOT NULL,
  [quantity] int NOT NULL,
  [time] datetimeoffset(7) NOT NULL
)", @"
EXEC sp_addextendedproperty
'MS_Description', N'商品编码',
'SCHEMA', N'StockTillSchema',
'TABLE', N'OperationLog',
'COLUMN', N'id'", @"
EXEC sp_addextendedproperty
'MS_Description', N'0为入库，1为出库',
'SCHEMA', N'StockTillSchema',
'TABLE', N'OperationLog',
'COLUMN', N'operation'", @"
EXEC sp_addextendedproperty
'MS_Description', N'数量',
'SCHEMA', N'StockTillSchema',
'TABLE', N'OperationLog',
'COLUMN', N'quantity'", @"
EXEC sp_addextendedproperty
'MS_Description', N'操作时间',
'SCHEMA', N'StockTillSchema',
'TABLE', N'OperationLog',
'COLUMN', N'time'" };
            foreach (string initSql in initSqls)
            {
                ExecuteNonQuery(initSql);
            }
        }
        public DataTable SelectAll()
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();

            using var adapter = new SqlDataAdapter("SELECT * FROM StockTillSchema.Goods", conn);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            return dataTable;
        }
        public DataRow? SelectById(string id)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();

            using var cmd = new SqlCommand($"SELECT * FROM StockTillSchema.Goods WHERE id = '{id}'", conn);

            using var adapter = new SqlDataAdapter(cmd);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            // 返回第一行（如果有）
            return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
        }
        public void Insert(string id, string name, int quantity, decimal cost, decimal price)
        {
            string sql = $"INSERT INTO StockTillSchema.Goods (id, name, quantity, cost, price) VALUES ('{id}', '{name}', {quantity}, {cost}, {price})";
            ExecuteNonQuery(sql);
        }

        public void UpdateById(string id, string name, int quantity, decimal cost, decimal price)
        {
            string sql = $"UPDATE StockTillSchema.Goods SET name = '{name}', quantity = {quantity}, cost = {cost}, price = {price} WHERE id = '{id}'";
            ExecuteNonQuery(sql);
        }
        public void ReduceQuantityById(string id,int reduce)
        {
            string sql = $"UPDATE StockTillSchema.Goods SET quantity = quantity - {reduce} WHERE id = '{id}'";
            ExecuteNonQuery(sql);
        }
        public void InsertLog(string id,bool isTill,int quantity)
        {
            string sql = $"INSERT INTO StockTillSchema.OperationLog (id, operation, quantity, time) VALUES ('{id}', '{isTill}', {quantity}, CAST('{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fffffff zzz}' AS DATETIMEOFFSET))";
            ExecuteNonQuery(sql);
        }
        public DataTable SelectLog(bool? isTill,DateTimeOffset startDate,DateTimeOffset endDate)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            string isTillSql = isTill == null ? "NULL" : (isTill.Value ? "1" : "0");
            string sql = $@"
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
  ({isTillSql} IS NULL OR operation = {isTillSql})
  AND (TIME >= '{startDate}' AND TIME <= '{endDate}')";
            //MessageBox.Show(sql);
            using var adapter = new SqlDataAdapter(sql, conn);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            return dataTable;
        }
        public bool ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            using var command = new SqlCommand(sql, conn);
            //command.Parameters.AddRange(parameters);
            return command.ExecuteScalar() != null;
        }
        public int ExecuteNonQuery(string sql)
        {
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();

            using var command = new SqlCommand(sql, conn);

            return command.ExecuteNonQuery();
        }
    }
}