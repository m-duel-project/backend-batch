#r "System.Data"

using System;
using System.Data.SqlClient;

public static void Run(string myBlob, TraceWriter log)
{
    log.Info("C# Blob trigger function processed");

    using (var connection = new SqlConnection(
        "Server=tcp:dwasqldb.database.windows.net,1433;Initial Catalog=m-duel-sql;Persist Security Info=False;User ID=konan@dwasqldb;Password=P@ssw0rd;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
            ))  // ���� ���ڿ��� �����ϰ� Azure SQL Database ������ ������ user id�� pwd�� ����. ������� DB���� �ڵ����� ���� ���ڿ��� ����
    {
        connection.Open();

        using (connection)
        {
            SqlCommand cmd = new SqlCommand();

            cmd.Connection = connection;
            cmd.CommandText = "INSERT INTO batch(blob)   VALUES(@blob)";
            cmd.Parameters.AddWithValue("@blob", myBlob);
            cmd.ExecuteNonQuery();
        }
        log.Info("Connected successfully.");
    }
}