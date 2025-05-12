using com.amtec.SQLConnect;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using Compal.MESComponent;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PartAssignment.com.amtec.SelectGW
{
    public partial class SelectGW
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private MySqlConnection con;
        private SqlConnection conn;
        private DBConnection dbconn;
        ExecutionResult exeRes;
        private OracleConnection oraconn;
        private IMSApiSessionContextStruct sessionContext;

        public SelectGW(IMSApiSessionContextStruct sessionContext)
        {
            this.sessionContext = sessionContext;
            exeRes = new ExecutionResult();
            dbconn = new DBConnection();
            conn = dbconn.GetSQLDbConnect();
            oraconn = dbconn.GetDBConnect();
            con = dbconn.GetMySQLDbConnect();
        }

        public ExecutionResult CheckWorkOrderContainSN(string order)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"SELECT * FROM GLO.CHARGE_SNR GL JOIN BDE.CHARGE CH ON (gl.CHARGE_NR = ch.CHARGE_NR) JOIN GLO.ADIS AD ON (ch.OBJECT_ID = ad.OBJECT_ID) where ch.charge_ext ='" +
                order +
                "'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult CheckWorkOrderExist(string order)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select * from bde.charge c where c.charge_ext ='" + order + "'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult CheckWorkOrderStatus(string order)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select c.bea_status from bde.charge c where c.charge_ext ='" + order + "'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult DeletePartLocation(
            string dbType,
            string part_no,
            string location_lager_id,
            string part_object_id)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            if(dbType == "SQL Server")
            {
                string sql = @"delete from ml.ADIS_LAGER WHERE OBJECT_ID = '" + part_object_id + "'";
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            } else if(dbType == "Oracle")
            {
                string sql = @"delete from ml.ADIS_LAGER WHERE OBJECT_ID = '" + part_object_id + "'";
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult GetBomSetupCount()
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"SELECT * FROM GLO.PROZ G
                           WHERE G.RUEST_FLAG='Y'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult GetBomSetupNUTCount()
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"SELECT * FROM glo.proz g
                           where g.komp_name like 'NUT%'
                           and G.RUEST_FLAG='J'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public string GetBurn(string dbType, string Version, string hsn)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string burn = string.Empty;
            if(dbType == "MySQL")
            {
                string sql = @"select node_data from ict where  serialNum = '" + hsn + "' and result=1 order by date_time limit 1";
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
                MySqlCommand command = new MySqlCommand();
                command.Connection = con;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }

            if(ds.Tables[0].Rows.Count > 0)
            {
                burn = ds.Tables[0].Rows[0][0].ToString();
            } else
            {
                exeRes.Status = false;
                burn = "ERROR";
            }

            return burn;
        }

        public async Task<string> GetBurnAsync(string dbType, string Version, string hsn)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string burn = string.Empty;
            if(dbType == "MySQL")
            {
                string sql = @"select node_data from ict where  serialNum = '" + hsn + "' and result=1 order by date_time limit 1";

                MySqlCommand command = new MySqlCommand();
                command.Connection = con;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    await command.Connection.OpenAsync();
                }
                using(MySqlDataAdapter dataAdapter = new MySqlDataAdapter())
                {
                    dataAdapter.SelectCommand = command;
                    dataAdapter.SelectCommand.CommandText = sql;
                    dataAdapter.Fill(ds);
                }
                command.Connection.Close();
            }

            if(ds.Tables[0].Rows.Count > 0)
            {
                burn = ds.Tables[0].Rows[0][0].ToString();
            } else
            {
                exeRes.Status = false;
                burn = "ERROR";
            }

            return burn;
        }

        public string GetEol(string dbType, string Version, string hsn)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string eol = string.Empty;
            if(dbType == "MySQL")
            {
                string sql = @"select node_data from normaltest where serialNum = '" +
                    hsn +
                    "' order by date_time limit 1";
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
                MySqlCommand command = new MySqlCommand();
                command.Connection = con;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }

            if(ds.Tables[0].Rows.Count > 0)
            {
                eol = ds.Tables[0].Rows[0][0].ToString();
            } else
            {
                exeRes.Status = false;
                eol = "ERROR";
            }

            return eol;
        }

        public string GetHigh(string dbType, string Version, string hsn)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string high = string.Empty;
            if(dbType == "MySQL")
            {
                string sql = @"select node_data from hightemtest where serialNum = '" +
                    hsn +
                    "' order by date_time limit 1";
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
                MySqlCommand command = new MySqlCommand();
                command.Connection = con;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }

            if(ds.Tables[0].Rows.Count > 0)
            {
                high = ds.Tables[0].Rows[0][0].ToString();
            } else
            {
                exeRes.Status = false;
                high = "ERROR";
            }

            return high;
        }

        public ExecutionResult GetKapID()
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select kap_id,kap_id_twin,kap_nr,kap_typ from bde.kast k";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult GetKapIDM(string kap_nr)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select kap_id from bde.kast where kap_typ = 'M' and kap_nr = '" + kap_nr + "'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult GetKapIDT(string kap_nr)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select kap_id from bde.kast where kap_typ = 'T' and kap_nr = '" + kap_nr + "'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public string GetLagerGrpID(string dbType, string cell_id, string group_no)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string group_id = string.Empty;
            if(dbType == "SQL Server")
            {
                string sql = @"SELECT LAGER_GRP_ID FROM ML.LAGER_GRP WHERE LAGER_GRP_NR = '" +
                    group_no +
                    "' AND STORAGE_CELL_ID = '" +
                    cell_id +
                    "' and WERK_ID = '3000000'";
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            } else if(dbType == "Oracle")
            {
                string sql = @"SELECT LAGER_GRP_ID FROM ML.LAGER_GRP WHERE LAGER_GRP_NR = '" +
                    group_no +
                    "' AND STORAGE_CELL_ID = '" +
                    cell_id +
                    "' and WERK_ID = '3000000'";
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                group_id = ds.Tables[0].Rows[0]["LAGER_GRP_ID"].ToString();
            } else
            {
                exeRes.Status = false;
            }

            return group_id;
        }

        public string GetLagerID(string dbType, string Version)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string lag_id = string.Empty;
            if(dbType == "SQL Server")
            {
                if(Version == "1")
                {
                    string sql = @"SELECT NEXT VALUE FOR ML.SEQ_LAGERORT";
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand();
                    command.Connection = conn;
                    if(!(command.Connection.State == ConnectionState.Open))
                    {
                        command.Connection.Open();
                    }
                    dataAdapter.SelectCommand = command;
                    dataAdapter.SelectCommand.CommandText = sql;
                    dataAdapter.Fill(ds);
                    command.Connection.Close();
                } else if(Version == "2")
                {
                    string sql = @"SELECT ID_VALUE + 1 From ML.SEQ_LAGERORT where ID_NAME = 'SEQ_LAGERORT'";
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand();
                    command.Connection = conn;
                    if(!(command.Connection.State == ConnectionState.Open))
                    {
                        command.Connection.Open();
                    }
                    dataAdapter.SelectCommand = command;
                    dataAdapter.SelectCommand.CommandText = sql;
                    dataAdapter.Fill(ds);
                    command.Connection.Close();
                }
            } else if(dbType == "Oracle")
            {
                string sql = @"SELECT ML.SEQ_LAGERORT.NEXTVAL from dual";
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                lag_id = ds.Tables[0].Rows[0][0].ToString();
            } else
            {
                exeRes.Status = false;
            }

            return lag_id;
        }

        public string GetLocationLagerID(string dbType, string Version, string location)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string location_lag_id = string.Empty;
            if(dbType == "SQL Server")
            {
                if(Version == "1")
                {
                    string sql = @"SELECT LAGER_ID FROM ML.LAGERORT L WHERE L.LAGER_NR = '" + location + "'";
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand();
                    command.Connection = conn;
                    if(!(command.Connection.State == ConnectionState.Open))
                    {
                        command.Connection.Open();
                    }
                    dataAdapter.SelectCommand = command;
                    dataAdapter.SelectCommand.CommandText = sql;
                    dataAdapter.Fill(ds);
                    command.Connection.Close();
                } else if(Version == "2")
                {
                    string sql = @"SELECT LAGER_ID FROM ML.LAGERORT L WHERE L.LAGER_NR = '" + location + "'";
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand();
                    command.Connection = conn;
                    if(!(command.Connection.State == ConnectionState.Open))
                    {
                        command.Connection.Open();
                    }
                    dataAdapter.SelectCommand = command;
                    dataAdapter.SelectCommand.CommandText = sql;
                    dataAdapter.Fill(ds);
                    command.Connection.Close();
                }
            } else if(dbType == "Oracle")
            {
                string sql = @"SELECT LAGER_ID FROM ML.LAGERORT L WHERE L.LAGER_NR = '" + location + "'";
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                location_lag_id = ds.Tables[0].Rows[0][0].ToString();
            } else
            {
                exeRes.Status = false;
                location_lag_id = "ERROR";
            }

            return location_lag_id;
        }

        public ExecutionResult GetMaGrpID()
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select ma_grp_id, ma_grp_id_twin, ma_grp_nr, ma_grp_typ from bde.ma_grp mg where ma_grp_nr <> 'DUMMY'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult GetMaGrpIDM(string ma_grp_nr)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select ma_grp_id from bde.ma_grp mg where mg.MA_GRP_TYP = 'M' and ma_grp_nr = '" +
                ma_grp_nr +
                "'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult GetMaGrpIDT(string ma_grp_nr)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select ma_grp_id from bde.ma_grp mg where mg.MA_GRP_TYP = 'T' and ma_grp_nr = '" +
                ma_grp_nr +
                "'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult GetMOData(string status)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select v.charge_ext as 生产工单,
       V.CHARGE_BEZ AS 工单描述,
       V.ARTIKEL    AS 生产料号,
       V.BEA_STATUS AS 工单状态
  from bde.v_auft v
 where V.bea_status = '" +
                status +
                "' ORDER BY V.charge_created DESC";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult GetNUTKom_id()
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select * from glo.sksl s
                           where s.komp_name like 'NUT%'
                           and s.komponente_id<>'0'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public string GetPartLagerID(string dbType, string Version, string part_no)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string part_lag_id = string.Empty;
            if(dbType == "SQL Server")
            {
                if(Version == "1")
                {
                    string sql = @"SELECT A.DEFAULT_LAGER_ID FROM GLO.ADIS A WHERE A.ARTIKEL = '" + part_no + "'";
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand();
                    command.Connection = conn;
                    if(!(command.Connection.State == ConnectionState.Open))
                    {
                        command.Connection.Open();
                    }
                    dataAdapter.SelectCommand = command;
                    dataAdapter.SelectCommand.CommandText = sql;
                    dataAdapter.Fill(ds);
                    command.Connection.Close();
                } else if(Version == "2")
                {
                    string sql = @"SELECT A.DEFAULT_LAGER_ID FROM GLO.ADIS A WHERE A.ARTIKEL = '" + part_no + "'";
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand();
                    command.Connection = conn;
                    if(!(command.Connection.State == ConnectionState.Open))
                    {
                        command.Connection.Open();
                    }
                    dataAdapter.SelectCommand = command;
                    dataAdapter.SelectCommand.CommandText = sql;
                    dataAdapter.Fill(ds);
                    command.Connection.Close();
                }
            } else if(dbType == "Oracle")
            {
                string sql = @"SELECT A.DEFAULT_LAGER_ID FROM GLO.ADIS A WHERE A.ARTIKEL = '" + part_no + "'";
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                part_lag_id = ds.Tables[0].Rows[0][0].ToString();
            } else
            {
                exeRes.Status = false;
                part_lag_id = "ERROR";
            }

            return part_lag_id;
        }

        public string GetPartLocation(string dbType, string part_no, string location_lager_id, string part_object_id)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string cout;
            if(dbType == "SQL Server")
            {
                string sql = @"select * from ml.ADIS_LAGER WHERE OBJECT_ID = '" + part_object_id + "'";
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            } else if(dbType == "Oracle")
            {
                string sql = @"select * from ml.ADIS_LAGER WHERE OBJECT_ID = '" + part_object_id + "'";
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                cout = "OK";
            } else
            {
                exeRes.Status = false;
                cout = "NG";
            }

            return cout;
        }

        public ExecutionResult GetPartNoOBJID(string partno)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select g.object_id from glo.adis g where g.artikel='" + partno + "'";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public string GetPartObjectID(string dbType, string Version, string part_no)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string part_object_id = string.Empty;
            if(dbType == "SQL Server")
            {
                if(Version == "1")
                {
                    string sql = @"SELECT A.OBJECT_ID FROM GLO.ADIS A WHERE A.ARTIKEL = '" + part_no + "'";
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand();
                    command.Connection = conn;
                    if(!(command.Connection.State == ConnectionState.Open))
                    {
                        command.Connection.Open();
                    }
                    dataAdapter.SelectCommand = command;
                    dataAdapter.SelectCommand.CommandText = sql;
                    dataAdapter.Fill(ds);
                    command.Connection.Close();
                } else if(Version == "2")
                {
                    string sql = @"SELECT A.OBJECT_ID FROM GLO.ADIS A WHERE A.ARTIKEL = '" + part_no + "'";
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand();
                    command.Connection = conn;
                    if(!(command.Connection.State == ConnectionState.Open))
                    {
                        command.Connection.Open();
                    }
                    dataAdapter.SelectCommand = command;
                    dataAdapter.SelectCommand.CommandText = sql;
                    dataAdapter.Fill(ds);
                    command.Connection.Close();
                }
            } else if(dbType == "Oracle")
            {
                string sql = @"SELECT A.OBJECT_ID FROM GLO.ADIS A WHERE A.ARTIKEL = '" + part_no + "'";
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                part_object_id = ds.Tables[0].Rows[0][0].ToString();
            } else
            {
                exeRes.Status = false;
                part_object_id = "ERROR";
            }

            return part_object_id;
        }

        public string GetPCBSN(string dbType, string Version, string hsn)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string pcb_sn = string.Empty;
            if(dbType == "MySQL")
            {
                string sql = @"SELECT bindingSerialNum FROM maketag WHERE serialNum =  '" + hsn + "'";
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
                MySqlCommand command = new MySqlCommand();
                command.Connection = con;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }

            if(ds.Tables[0].Rows.Count > 0)
            {
                pcb_sn = ds.Tables[0].Rows[0][0].ToString();
            } else
            {
                exeRes.Status = false;
                pcb_sn = "ERROR";
            }

            return pcb_sn;
        }

        public string GetSEQID(string dbType, string Version, string sequenceName)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string seq_id = string.Empty;
            if(dbType == "SQL Server")
            {
                if(Version == "1")
                {
                    string tempName = "x" + sequenceName;
                    string sql = string.Format("SELECT next value for {0}", tempName);
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand();
                    command.Connection = conn;
                    if(!(command.Connection.State == ConnectionState.Open))
                    {
                        command.Connection.Open();
                    }
                    dataAdapter.SelectCommand = command;
                    dataAdapter.SelectCommand.CommandText = sql;
                    dataAdapter.Fill(ds);
                    command.Connection.Close();
                } else if(Version == "2")
                {
                    string tempName = "x" + sequenceName;
                    string[] strs = tempName.Split(new char[] { '.' });
                    string sql = string.Format("select ID_VALUE from {0} where ID_NAME='{1}'", tempName, strs[1]);
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                    SqlCommand command = new SqlCommand();
                    command.Connection = conn;
                    if(!(command.Connection.State == ConnectionState.Open))
                    {
                        command.Connection.Open();
                    }
                    dataAdapter.SelectCommand = command;
                    dataAdapter.SelectCommand.CommandText = sql;
                    dataAdapter.Fill(ds);
                    command.Connection.Close();
                }
            } else if(dbType == "Oracle")
            {
                string sql = string.Format("SELECT {0}.NEXTVAL FROM dual", sequenceName);
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                seq_id = ds.Tables[0].Rows[0][0].ToString();
            } else
            {
                exeRes.Status = false;
            }

            return seq_id;
        }

        public string GetStateID(string dbType)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string state_id = string.Empty;
            if(dbType == "SQL Server")
            {
                string sql = @"SELECT ID FROM ML.STORAGE_STATE WHERE CODE = 'F' and plant_id = '3000000'";
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            } else if(dbType == "Oracle")
            {
                string sql = @"SELECT ID FROM ML.STORAGE_STATE WHERE CODE = 'F' and plant_id = '3000000'";
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                state_id = ds.Tables[0].Rows[0]["ID"].ToString();
            } else
            {
                exeRes.Status = false;
            }

            return state_id;
        }

        public ExecutionResult GetStorageCell(string dbType)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"SELECT CELL_NUMBER FROM ml.storage_cell where CELL_NUMBER <> 'Dummy' order by CELL_NUMBER";
            if(dbType == "SQL Server")
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(dbType == "Oracle")
            {
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public string GetStorageCellID(string dbType, string cell_no)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string cell_id = string.Empty;
            if(dbType == "SQL Server")
            {
                string sql = @"select ID from ml.STORAGE_CELL where CELL_NUMBER = '" +
                    cell_no +
                    "' AND PLANT_ID = '3000000'";
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            } else if(dbType == "Oracle")
            {
                string sql = @"select ID from ml.STORAGE_CELL where CELL_NUMBER = '" +
                    cell_no +
                    "' AND PLANT_ID = '3000000'";
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                cell_id = ds.Tables[0].Rows[0]["ID"].ToString();
            } else
            {
                exeRes.Status = false;
            }

            return cell_id;
        }

        public ExecutionResult GetStorageGroup(string dbType, string cell_id)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"SELECT LAGER_GRP_NR FROM ML.LAGER_GRP WHERE STORAGE_CELL_ID IN (SELECT ID FROM ml.storage_cell WHERE CELL_NUMBER = '" +
                cell_id +
                "')";
            if(dbType == "SQL Server")
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(dbType == "Oracle")
            {
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public string GetTypeID(string dbType)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string type_id = string.Empty;
            if(dbType == "SQL Server")
            {
                string sql = @"SELECT ID FROM ML.STORAGE_TYPE WHERE CODE = 'M' and plant_id = '3000000'";
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            } else if(dbType == "Oracle")
            {
                string sql = @"SELECT ID FROM ML.STORAGE_TYPE WHERE CODE = 'M' and plant_id = '3000000'";
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                type_id = ds.Tables[0].Rows[0]["ID"].ToString();
            } else
            {
                exeRes.Status = false;
            }

            return type_id;
        }

        public string GetUserID(string dbType)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string user_id = string.Empty;
            if(dbType == "SQL Server")
            {
                string sql = @"SELECT PERS_ID FROM BDE.PERS_STAMM WHERE BDE_USERID = 'ADMIN' and werk_id= '3000000'";
                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            } else if(dbType == "Oracle")
            {
                string sql = @"SELECT PERS_ID FROM BDE.PERS_STAMM WHERE BDE_USERID = 'ADMIN' and werk_id= '3000000";
                OracleDataAdapter dataAdapter = new OracleDataAdapter();
                OracleCommand command = new OracleCommand();
                command.Connection = oraconn;
                if(!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                dataAdapter.SelectCommand = command;
                dataAdapter.SelectCommand.CommandText = sql;
                dataAdapter.Fill(ds);
                command.Connection.Close();
            }
            if(ds.Tables[0].Rows.Count > 0)
            {
                user_id = ds.Tables[0].Rows[0]["PERS_ID"].ToString();
            } else
            {
                exeRes.Status = false;
            }

            return user_id;
        }

        public ExecutionResult MaxAdisRefID(int SeqNo)
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select max(id) from  glo.adis_ref ";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }

        public ExecutionResult MaxAdisRefSeqNo()
        {
            exeRes.Status = true;
            DataSet ds = new DataSet();
            string sql = @"select max(seq_no) from glo.adis_ref  order by seq_no desc";
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            if(!(command.Connection.State == ConnectionState.Open))
            {
                command.Connection.Open();
            }
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.CommandText = sql;
            dataAdapter.Fill(ds);
            command.Connection.Close();
            if(ds.Tables[0].Rows.Count > 0)
            {
                exeRes.Anything = ds;
                exeRes.Status = true;
            } else
            {
                exeRes.Status = false;
            }

            return exeRes;
        }
    }
}
