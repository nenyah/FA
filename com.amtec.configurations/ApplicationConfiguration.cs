using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using com.amtec.action;

namespace com.amtec.configurations
{
    public class ApplicationConfiguration
    {

        public String Board_Module { get; set; }

        public String StationNumber { get; set; }

        public String Client { get; set; }

        public String LogInType { get; set; }

        public String RegistrationType { get; set; }

        public String SerialPort { get; set; }

        public String BaudRate { get; set; }

        public String Parity { get; set; }

        public String StopBits { get; set; }

        public String DataBits { get; set; }

        public String NewLineSymbol { get; set; }

        public String High { get; set; }

        public String Low { get; set; }

        public String EndCommand { get; set; }

        public String IPAddress { get; set; }

        public String Port { get; set; }

        public String DBType { get; set; }

        public int RefreshTimeSpan { get; set; }

        public String AUTH_TEAM { get; set; }

        public String LockMat { get; set; }

        public String lblCaptionsize { get; set; }

        public String PlantNo { get; set; }

        public String Version { get; set; }

        public String Merge { get; set; }

        public String UploadRecipe { get; set; }

        public String COM_PORT { get; set; }

        public String CUSTOM_CODE { get; set; }

        public string REGEX_PATTERN { get; set; }

        public ApplicationConfiguration()
        {
            string filePath = Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string _appDir = Path.GetDirectoryName(filePath);
            XDocument config = XDocument.Load(_appDir + @"\ApplicationConfig.xml");

            StationNumber = GetParameterValues(config, "StationNumber");
            Client = GetParameterValues(config, "Client");
            RegistrationType = GetParameterValues(config, "RegistrationType");
            LogInType = GetParameterValues(config, "LogInType");
            AUTH_TEAM = GetParameterValues(config, "AUTH_TEAM");
            IPAddress = GetParameterValues(config, "IPAddress");
            Port = GetParameterValues(config, "Port");
            RefreshTimeSpan = GetIntValue(GetParameterValues(config, "RefreshTimeSpan"));
            lblCaptionsize = GetParameterValues(config, "lblCaptionsize");
            PlantNo = GetParameterValues(config, "PlantNo");
            DBType = GetParameterValues(config, "DBType");
            Version = GetParameterValues(config, "Version");
            Merge = GetParameterValues(config, "Merge");
            UploadRecipe = GetParameterValues(config, "UploadRecipe");
            SerialPort = GetParameterValues(config, "SerialPort");
            BaudRate = GetParameterValues(config, "BaudRate");
            Parity = GetParameterValues(config, "Parity");
            StopBits = GetParameterValues(config, "StopBits");
            DataBits = GetParameterValues(config, "DataBits");
            NewLineSymbol = GetParameterValues(config, "NewLineSymbol");
            High = GetParameterValues(config, "High");
            Low = GetParameterValues(config, "Low");
            EndCommand = GetParameterValues(config, "EndCommand");
            Board_Module = GetParameterValues(config, "Board_Module");
            CUSTOM_CODE = GetParameterValues(config, "CUSTOM_CODE");
            REGEX_PATTERN = GetParameterValues(config, "REGEX_PATTERN");
        }

        private string GetParameterValues(XDocument config, string parameterName)
        {
            string value = null;
            if (config.Descendants(parameterName).FirstOrDefault() == null)
            {
                value = "";
                LogHelper.Error("The parameter " + parameterName + " can't find in the configuration file");
            }
            else
            {
                value = config.Descendants(parameterName).FirstOrDefault().Value;
            }
            return value;
        }

        private int GetIntValue(string text)
        {
            int value = 0;
            if (!string.IsNullOrEmpty(text))
            {
                value = Convert.ToInt32(text);
            }
            return value;
        }
    }
}
