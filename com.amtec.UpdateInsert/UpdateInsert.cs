using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Compal.MESComponent;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using com.amtec.action;
using com.amtec.configurations;
using com.amtec.SQLConnect;
using FA_COATING.com.amtec.Bean;
using System.IO;
using System.Reflection;
using System.IO.Ports;
using System.Threading;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace PartAssignment.com.amtec.UpdateInsert
{
    public partial class UpdateInsert
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private SqlConnection conn;
        private DBConnection dbconn;
        private OracleConnection oraconn;
        private BeanInfo beanInfo = new BeanInfo();
        //ExecutionResult exeRes;

        public UpdateInsert(IMSApiSessionContextStruct sessionContext)
        {
            this.sessionContext = sessionContext;
            //this.init = init;
            //this.view = view;
            //exeRes = new ExecutionResult();
            dbconn = new DBConnection();
            conn = dbconn.GetSQLDbConnect();
            oraconn = dbconn.GetDBConnect();

        }

        public ExecutionResult InsertPartAssign(int seqno, int id, int obj,double qty,int packobj)
        {

            ExecutionResult exeRes = new ExecutionResult();
            exeRes.Status = true;
            DataSet ds = new DataSet();
            //DateTime date1 = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            //DateTime date2 = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            string date1 = DateTime.Now.ToString();
            string date2 = DateTime.Now.ToString();

            if (exeRes.Status)
            {
                OracleTransaction transaction = null;
                try
                {
                    oraconn.Open();
                    transaction = oraconn.BeginTransaction();
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = oraconn;
                    cmd.Transaction = transaction;

                    string sql = null;

                    sql = @"insert into glo.adis_ref
                            (object_id,
                             object_id_ref,
                             werk_id,
                             ref_typ,
                             menge,
                             created,
                             user_id,
                             stamp,
                             artgrp_id,
                             check_mode,
                             pack_no,
                             adis_ref_child_id,
                             seq_no,
                             id)
                           values
                          ('" + obj + "','" + packobj + "','3000000','S','" + qty + "',to_date('" + date1 + "','yyyy/MM/dd hh24:mi:ss'),'1',to_date('" + date2 + "','yyyy/MM/dd hh24:mi:ss'),'0','0','','','" + seqno + "','" + id + "')";
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    exeRes.Status = true;

                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }
                    exeRes.Message = ex.Message;
                    exeRes.Status = false;
                }
                finally
                {
                    if (oraconn.State == ConnectionState.Open)
                    {
                        oraconn.Close();
                    }
                }
            }

            return exeRes;
        }

        public ExecutionResult updateBomSetupInfo(string updateflag)
        {
            ExecutionResult exeRes = new ExecutionResult();
            DataSet ds = new DataSet();
            string sql = @"update GLO.PROZ G SET G.RUEST_FLAG='"+ updateflag +"' WHERE G.RUEST_FLAG='Y'";
            OracleTransaction transaction = null;
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            try
            {
                command.Connection = oraconn;
                if (!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                command.CommandText = sql;
                command.ExecuteNonQuery();

                exeRes.Status = true;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                command.Connection.Close();
                exeRes.Status = false;
            }
            finally
            {
                command.Connection.Close();
            }

            return exeRes;
 
        }

        public ExecutionResult updateMaID(string m_id,string ma_grp_nr,string ma_grp_typ)
        {
            ExecutionResult exeRes = new ExecutionResult();
            DataSet ds = new DataSet();
            string sql = @"update bde.MA_GRP set MA_GRP_ID_TWIN = '" + m_id + "' where MA_GRP_NR ='" + ma_grp_nr + "'  and MA_GRP_TYP = '" + ma_grp_typ + "'";
            OracleTransaction transaction = null;
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            try
            {
                command.Connection = oraconn;
                if (!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                command.CommandText = sql;
                command.ExecuteNonQuery();

                exeRes.Status = true;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                command.Connection.Close();
                exeRes.Status = false;
            }
            finally
            {
                command.Connection.Close();
            }

            return exeRes;

        }

        public ExecutionResult updateKapID(string k_id, string kap_nr, string kap_typ)
        {
            ExecutionResult exeRes = new ExecutionResult();
            DataSet ds = new DataSet();
            string sql = @"update bde.kast set kap_id_twin = '" + k_id + "' where kap_nr ='" + kap_nr + "'  and kap_typ = '" + kap_typ + "'";
            OracleTransaction transaction = null;
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            try
            {
                command.Connection = oraconn;
                if (!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                command.CommandText = sql;
                command.ExecuteNonQuery();

                exeRes.Status = true;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                command.Connection.Close();
                exeRes.Status = false;
            }
            finally
            {
                command.Connection.Close();
            }

            return exeRes;

        }

        public ExecutionResult updateBomSetupNUTInfo()
        {
            ExecutionResult exeRes = new ExecutionResult();
            DataSet ds = new DataSet();
            string sql = @"update glo.proz g set g.ruest_flag='N'
                           where g.komp_name like 'NUT%'
                           and G.RUEST_FLAG='J'";
            OracleTransaction transaction = null;
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            try
            {
                command.Connection = oraconn;
                if (!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                command.CommandText = sql;
                command.ExecuteNonQuery();

                exeRes.Status = true;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                command.Connection.Close();
                exeRes.Status = false;
            }
            finally
            {
                command.Connection.Close();
            }

            return exeRes;

        }

        public ExecutionResult UpdateMOStatus(string mo)
        {
            ExecutionResult exeRes = new ExecutionResult();
            DataSet ds = new DataSet();
            string sql = @"update bde.v_auft v set v.bea_status='E' where v.charge_ext='"+ mo + "'";
            OracleTransaction transaction = null;
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            try
            {
                command.Connection = oraconn;
                if (!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                command.CommandText = sql;
                command.ExecuteNonQuery();

                exeRes.Status = true;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                command.Connection.Close();
                exeRes.Status = false;
            }
            finally
            {
                command.Connection.Close();
            }

            return exeRes;
        }

        public ExecutionResult UpdateNUTKom_id()
        {
            ExecutionResult exeRes = new ExecutionResult();
            DataSet ds = new DataSet();
            string sql = @"UPDATE glo.sksl s SET s.komponente_id='0'
                           where s.komp_name like 'NUT%'
                           and s.komponente_id<>'0'";
            OracleTransaction transaction = null;
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            try
            {
                command.Connection = oraconn;
                if (!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                command.CommandText = sql;
                command.ExecuteNonQuery();

                exeRes.Status = true;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                command.Connection.Close();
                exeRes.Status = false;
            }
            finally
            {
                command.Connection.Close();
            }

            return exeRes;
        }

        public ExecutionResult UpdateMOStatusToCreate(string mo)
        {
            ExecutionResult exeRes = new ExecutionResult();
            DataSet ds = new DataSet();
            string sql = @"update bde.v_auft v set v.bea_status='F' where v.charge_ext='" + mo + "'";
            OracleTransaction transaction = null;
            OracleDataAdapter dataAdapter = new OracleDataAdapter();
            OracleCommand command = new OracleCommand();
            command.Connection = oraconn;
            try
            {
                command.Connection = oraconn;
                if (!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                command.CommandText = sql;
                command.ExecuteNonQuery();

                exeRes.Status = true;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                command.Connection.Close();
                exeRes.Status = false;
            }
            finally
            {
                command.Connection.Close();
            }

            return exeRes;
        }

        public ExecutionResult UpdateLagerID(string lag_id)
        {
            ExecutionResult exeRes = new ExecutionResult();
            DataSet ds = new DataSet();
            string sql = @"update  ML.SEQ_LAGERORT set ID_VALUE = '"+ lag_id + "' where ID_NAME = 'SEQ_LAGERORT'";
            SqlTransaction transaction = null;
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            SqlCommand command = new SqlCommand();
            command.Connection = conn;
            try
            {
                command.Connection = conn;
                if (!(command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Open();
                }
                command.CommandText = sql;
                command.ExecuteNonQuery();

                exeRes.Status = true;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                command.Connection.Close();
                exeRes.Status = false;
            }
            finally
            {
                command.Connection.Close();
            }

            return exeRes;
        }

        public ExecutionResult InsertLocation(string dbtype,string location_no,string location_desc,string user_id,string type_id,string state_id,string lag_id,string group_id)
        {

            ExecutionResult exeRes = new ExecutionResult();
            exeRes.Status = true;
            DataSet ds = new DataSet();
            DateTime date1 = Convert.ToDateTime("1970-01-01 08:00:00.0000000");
            //DateTime date2 = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            //string date1 = DateTime.Now.ToString();
            //string date2 = DateTime.Now.ToString();

            if (exeRes.Status)
            {
                if (dbtype == "Oracle")
                {
                    OracleTransaction transaction = null;
                    try
                    {
                        oraconn.Open();
                        transaction = oraconn.BeginTransaction();
                        OracleCommand cmd = new OracleCommand();
                        cmd.Connection = oraconn;
                        cmd.Transaction = transaction;

                        string sql = null;

                        sql = @"INSERT INTO ml.lagerort (AKTIV, BARCODE, CREATED, HAS_ATTRIBUTES, INV_DATE, INV_START_DATE, LAGER_BEZ, LAGER_GRP_ID, LAGER_ID, LAGER_NR, MOBILE, STAMP, STOP_POINT_ID, STORAGE_PART_ATTACHED, STORAGE_STATE_ID, STORAGE_TYPE_ID, USER_ID, WERK_ID) 
                                values('1','',SYSDATE,'0','1970-01-01 08:00:00.0000000',NULL,'" + location_desc + "'," + group_id + ",'" + lag_id + "','" + location_no + "','0',SYSDATE,NULL,NULL,'" + state_id + "','" + type_id + "','" + user_id + "','3000000')";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        exeRes.Status = true;

                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                        exeRes.Message = ex.Message;
                        exeRes.Status = false;
                    }
                    finally
                    {
                        if (oraconn.State == ConnectionState.Open)
                        {
                            oraconn.Close();
                        }
                    }
                }
                else if (dbtype == "SQL Server")
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.Transaction = transaction;

                        string sql = null;

                        sql = @"INSERT INTO ml.lagerort (AKTIV, BARCODE, CREATED, HAS_ATTRIBUTES, INV_DATE, INV_START_DATE, LAGER_BEZ, LAGER_GRP_ID, LAGER_ID, LAGER_NR, MOBILE, STAMP, STOP_POINT_ID, STORAGE_PART_ATTACHED, STORAGE_STATE_ID, STORAGE_TYPE_ID, USER_ID, WERK_ID) 
                                values('1','',GETDATE(),'0','1970-01-01 08:00:00.0000000',NULL,'" + location_desc + "'," + group_id + ",'" + lag_id + "','" + location_no + "','0',GETDATE(),NULL,NULL,'" + state_id + "','" + type_id + "','" + user_id + "','3000000')";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        exeRes.Status = true;

                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                        exeRes.Message = ex.Message;
                        exeRes.Status = false;
                    }
                    finally
                    {
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                    }
                }
            }

            return exeRes;
        }

        public ExecutionResult InsertPartLocation(string dbtype, string part_no, string location,string part_object_id)
        {

            ExecutionResult exeRes = new ExecutionResult();
            exeRes.Status = true;
            DataSet ds = new DataSet();
            //DateTime date1 = Convert.ToDateTime("1970-01-01 08:00:00.0000000");
            //DateTime date2 = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            string date1 = DateTime.Now.ToString();
            string date2 = DateTime.Now.ToString();
            if (exeRes.Status)
            {
                if (dbtype == "Oracle")
                {
                    OracleTransaction transaction = null;
                    try
                    {
                        oraconn.Open();
                        transaction = oraconn.BeginTransaction();
                        OracleCommand cmd = new OracleCommand();
                        cmd.Connection = oraconn;
                        cmd.Transaction = transaction;

                        string sql = null;

                        sql = @"insert into ml.ADIS_LAGER(CLIENT_ID,COMPANY_ID,CREATED,LAGER_ID,LFD_NR,OBJECT_ID,STAMP,USER_ID,WERK_ID) 
                                VALUES('1','1','"+date1+"','" + location + "','0','" + part_object_id + "','"+date2+"','1','3000000')";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        exeRes.Status = true;

                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                        exeRes.Message = ex.Message;
                        exeRes.Status = false;
                    }
                    finally
                    {
                        if (oraconn.State == ConnectionState.Open)
                        {
                            oraconn.Close();
                        }
                    }
                }
                else if (dbtype == "SQL Server")
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.Transaction = transaction;

                        string sql = null;

                        sql = @"insert into ml.ADIS_LAGER(CLIENT_ID,COMPANY_ID,CREATED,LAGER_ID,LFD_NR,OBJECT_ID,STAMP,USER_ID,WERK_ID) 
                                VALUES('1','1',GETDATE(),'" + location + "','0','"+part_object_id+"',GETDATE(),'1','3000000')";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        exeRes.Status = true;

                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                        exeRes.Message = ex.Message;
                        exeRes.Status = false;
                    }
                    finally
                    {
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                    }
                }
            }

            return exeRes;
        }

        public ExecutionResult UpdatePartDefautLocation(string dbtype, string part_no, string location)
        {

            ExecutionResult exeRes = new ExecutionResult();
            exeRes.Status = true;
            DataSet ds = new DataSet();
            if (exeRes.Status)
            {
                if (dbtype == "Oracle")
                {
                    OracleTransaction transaction = null;
                    try
                    {
                        oraconn.Open();
                        transaction = oraconn.BeginTransaction();
                        OracleCommand cmd = new OracleCommand();
                        cmd.Connection = oraconn;
                        cmd.Transaction = transaction;

                        string sql = null;

                        sql = @"update GLO.ADIS  set DEFAULT_LAGER_ID = '"+ location+"' WHERE ARTIKEL = '"+ part_no+"'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        exeRes.Status = true;

                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                        exeRes.Message = ex.Message;
                        exeRes.Status = false;
                    }
                    finally
                    {
                        if (oraconn.State == ConnectionState.Open)
                        {
                            oraconn.Close();
                        }
                    }
                }
                else if (dbtype == "SQL Server")
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.Transaction = transaction;

                        string sql = null;

                        sql = @"update GLO.ADIS  set DEFAULT_LAGER_ID = '" + location + "' WHERE ARTIKEL = '" + part_no + "'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                        exeRes.Status = true;

                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                        exeRes.Message = ex.Message;
                        exeRes.Status = false;
                    }
                    finally
                    {
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                    }
                }
            }

            return exeRes;
        }
    }
}
