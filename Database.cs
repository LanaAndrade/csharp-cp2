using System.Data.SqlClient;

namespace LojaApp;

public static class Database
{
private const string ConnStr =
    "Server=IBM-PE0AQNJS\\SQLEXPRESS;Database=LojaDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection GetOpenConnection()
    {
        var conn = new SqlConnection(ConnStr);
        conn.Open();
        return conn;
    }
}
