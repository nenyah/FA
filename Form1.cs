using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
 

namespace SendPartno
{
    public partial class Form1 : Form
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext=null;
        public Form1(string userName, string password, DateTime dTime, IMSApiSessionContextStruct _sessionContext)
        {
            InitializeComponent();
            sessionContext = _sessionContext;
        }
       
        private void button1_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(this.textBox1.Text))
            {
                int error = 0;
                GetPartnodetail(this.textBox1.Text, out error);
            }
            
        }
        public void errorHandler(int typeOfError, string logMessage)
        {


            String errorBuilder = null;
            String isSucces = null;
            switch (typeOfError)
            {
                case 0:
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.SelectionColor = Color.Black;
                        isSucces = "SUCCESS";
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + "  " + logMessage + "\n";
                      
                    }));

                    break;
                case 1:
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.SelectionColor = Color.Red;
                        isSucces = "FAIL";
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + "  " + logMessage + "\n";
                      
                    }));
                    break;
                default:
                    richTextBox1.SelectionColor = Color.Red;
                    isSucces = "FAIL";
                    errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + "  " + logMessage + "\n";
                    break; ;
            }
            this.Invoke(new MethodInvoker(delegate
            {
                richTextBox1.AppendText(errorBuilder);
                richTextBox1.ScrollToCaret();
            }));

        }


        public void GetPartnodetail(string partno, out int error)
        {
            string errorMsg = "";
            // MaterialBinEntity entity = new MaterialBinEntity();
            KeyValue[] materialBinFilters = new KeyValue[] { new KeyValue("MATERIAL_BIN_PART_NUMBER", partno) };
            string[] materialBinResultKeys = new string[] { "MATERIAL_BIN_NUMBER"};
            string[] materialBinResultValues = new string[] { };
            AttributeInfo[] attr = new AttributeInfo[] { };
            // LogHelper.Info("begin api mlGetMaterialBinData ");
            error = imsapi.mlGetMaterialBinData(sessionContext, "XS1D-S080-01", materialBinFilters, attr, materialBinResultKeys, out materialBinResultValues);
            imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            //LogHelper.Info("end api mlGetMaterialBinData (errorcode = " + error + ")");
            if (error == 0)
            {
                errorHandler(0,"get partno detaill success");
            }
        }

        public void GetMaterialBinEntity(string materialBinNumber, out int error)
        {
            string errorMsg = "";
           // MaterialBinEntity entity = new MaterialBinEntity();
            KeyValue[] materialBinFilters = new KeyValue[] { new KeyValue("MATERIAL_BIN_NUMBER", materialBinNumber) };
            string[] materialBinResultKeys = new string[] { "DATE_CHANGED", "DATE_CREATED", "EXPIRATION_DATE", "LOCK_STATE", "MATERIAL_BIN_DATE_CODE", "MATERIAL_BIN_NUMBER", "MATERIAL_BIN_PART_NUMBER"
            ,"MATERIAL_BIN_QTY_ACTUAL","MATERIAL_BIN_QTY_TOTAL","MATERIAL_BIN_STATE","MSL_FLOOR_LIFETIME_REMAIN","UNIT","MSL_STATE","PART_DESC","STORAGE_GROUP"
            ,"STORAGE_NUMBER","SUPPLIER_CHARGE_NUMBER","SUPPLIER_NAME","SUPPLIER_NUMBER"};
            string[] materialBinResultValues = new string[] { };
            AttributeInfo[] attr = new AttributeInfo[] { };
           // LogHelper.Info("begin api mlGetMaterialBinData ");
            error = imsapi.mlGetMaterialBinData(sessionContext, "XS1D-S080-01", materialBinFilters, attr, materialBinResultKeys, out materialBinResultValues);
            imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            //LogHelper.Info("end api mlGetMaterialBinData (errorcode = " + error + ")");
            if (error == 0)
            {
                if (materialBinResultValues.Length > 0)
                {
                    //entity.DATE_CHANGED = materialBinResultValues[0];
                    //entity.DATE_CREATED = materialBinResultValues[1];
                    //entity.EXPIRATION_DATE = materialBinResultValues[2];
                    //entity.LOCK_STATE = materialBinResultValues[3];
                    //entity.MATERIAL_BIN_DATE_CODE = materialBinResultValues[4];
                    //entity.MATERIAL_BIN_NUMBER = materialBinResultValues[5];
                    //entity.MATERIAL_BIN_PART_NUMBER = materialBinResultValues[6];
                    //entity.MATERIAL_BIN_QTY_ACTUAL = Convert.ToString(ChangeDataToD(materialBinResultValues[7].ToUpper()));
                    //entity.MATERIAL_BIN_QTY_TOTAL = Convert.ToString(ChangeDataToD(materialBinResultValues[8].ToUpper()));
                    //entity.MATERIAL_BIN_STATE = materialBinResultValues[9];
                    //entity.MSL_FLOOR_LIFETIME_REMAIN = materialBinResultValues[10];
                    //entity.MSL_LEVEL = materialBinResultValues[11];
                    //entity.MSL_STATE = materialBinResultValues[12];
                    //entity.PART_DESC = materialBinResultValues[13];
                    //entity.STORAGE_GROUP = materialBinResultValues[14];
                    //entity.STORAGE_NUMBER = materialBinResultValues[15];
                    //entity.SUPPLIER_CHARGE_NUMBER = materialBinResultValues[16];
                    //entity.SUPPLIER_NAME = materialBinResultValues[17];
                    //entity.SUPPLIER_NUMBER = materialBinResultValues[18];
                }
            }
            else
            {
                errorHandler(2, "获取MBN信息失败");
            }
        }
    }
}
