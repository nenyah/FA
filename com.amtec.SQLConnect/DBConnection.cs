using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.OracleClient;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace com.amtec.SQLConnect
{
    public class DBConnection
    {
        private string serverIp = null;
        private string port = null;
        private string serverName = null;
        private string uid = null;
        private string pw = null;
        private string SQLServerIp = null;
        private string SQLServerport = null;
        private string SQLserverName = null;
        //构造MySQL
        private string MySQLServerIp = null;
        private string MySQLServerport = null;
        private string MySQLserverName = null;

        //构造函数，从配置文件app.config中读取数据库连接
        public DBConnection() //初始化函数
        {
            serverIp = ConfigurationManager.AppSettings["SERVERIP"];
            port = ConfigurationManager.AppSettings["PORT"];
            serverName = ConfigurationManager.AppSettings["SERVERNAME"];

            SQLServerIp = ConfigurationManager.AppSettings["SQLSERVERIP"];
            SQLServerport = ConfigurationManager.AppSettings["SQLPORT"];
            SQLserverName = ConfigurationManager.AppSettings["SQLSERVERNAME"];

            MySQLServerIp = ConfigurationManager.AppSettings["MYSQLSERVERIP"];
            MySQLServerport = ConfigurationManager.AppSettings["MYSQLPORT"];
            MySQLserverName = ConfigurationManager.AppSettings["MYSQLSERVERNAME"];
            //uid = ConfigurationManager.AppSettings["UID"];
            //pw = ConfigurationManager.AppSettings["PW"];
            uid ="bde";
            pw = "bde_pwd";
        }

        public OracleConnection GetDBConnect()
        {
            string Connect = "Server =(DESCRIPTION =(ADDRESS_LIST =(ADDRESS = (PROTOCOL = TCP)" +
                "(HOST =" + serverIp + ")(PORT = " + port + "))) (CONNECT_DATA = (SERVICE_NAME =" + serverName + ") ) )" +
                ";uid=" + uid + ";pwd=" + pw + ";Persist Security Info=True";
            OracleConnection conn = new OracleConnection(Connect);
            return conn;
        }

        public SqlConnection GetSQLDbConnect()
        {
            string strConnect = @"Data Source=" + SQLServerIp + ";Initial Catalog=" + SQLserverName + ";User ID=" + uid + ";Password=" + pw;
            SqlConnection connect = new SqlConnection(strConnect);
            return connect;
        }

        public MySqlConnection GetMySQLDbConnect()
        {
            string strConn = @"server=" + MySQLServerIp + ";port=" + MySQLServerport + ";user=root;password=Zhou@7113585;database=" + MySQLserverName;
            MySqlConnection con = new MySqlConnection(strConn);
            return con;
        }
    }
}
