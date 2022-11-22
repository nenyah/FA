using com.amtec.forms;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System;
using System.Globalization;
using System.Text;

namespace com.amtec.action
{
    public class UtilityFunction
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private string StationNumber = "";

        public UtilityFunction(IMSApiSessionContextStruct sessionContext, string stationNo)
        {
            this.sessionContext = sessionContext;
            this.StationNumber = stationNo;
        }

        public DateTime GetServerDateTime()
        {
            var calendarDataResultKeys = new string[] { "CURRENT_TIME_MILLIS" };
            var calendarDataResultValues = new string[] { };
            int error = imsapi.mdataGetCalendarData(sessionContext, StationNumber, calendarDataResultKeys, out calendarDataResultValues);
            if (error != 0)
            {
                LogHelper.Info("API mdataGetCalendarData error code = " + error);
                return DateTime.Now;
            }
            long numer = long.Parse(calendarDataResultValues[0]);
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddMilliseconds(numer).ToLocalTime();
            return date;
        }

        /// <summary>
        /// 字符串编码转换
        /// </summary>
        /// <param name="srcEncoding">原编码</param>
        /// <param name="dstEncoding">目标编码</param>
        /// <param name="srcBytes">原字符串</param>
        /// <returns>字符串</returns>
        public static string TransferEncoding(Encoding srcEncoding, Encoding dstEncoding, string srcStr)
        {
            byte[] srcBytes = srcEncoding.GetBytes(srcStr);
            byte[] bytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);
            return dstEncoding.GetString(bytes);
        }

        /// <summary>
        /// 字节数组转为字符串
        /// 将指定的字节数组的每个元素的数值转换为它的等效十六进制字符串表示形式。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string BitToString(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            //将指定的字节数组的每个元素的数值转换为它的等效十六进制字符串表示形式。
            return BitConverter.ToString(bytes);
        }

        /// <summary>
        /// 将十六进制字符串转为字节数组
        /// </summary>
        /// <param name="bitStr"></param>
        /// <returns></returns>
        public static byte[] FromBitString(string bitStr)
        {
            if (bitStr == null)
            {
                return null;
            }

            string[] sInput = bitStr.Split("-".ToCharArray());
            byte[] data = new byte[sInput.Length];
            for (int i = 0; i < sInput.Length; i++)
            {
                data[i] = byte.Parse(sInput[i], NumberStyles.HexNumber);
            }

            return data;
        }
        //Encoding.UTF8.GetString(FromBitString(result)); 

        public string GetTeamNumberByUser(string userName)
        {
            string teamNo = "";
            int resultCode = -1;
            bool hasMore = false;
            string[] mdataGetUserDataKeys = new string[] { "TEAM_NUMBER" };
            string[] mdataGetUserDataValues = new string[] { };
            KeyValue[] mdataGetUserDataFilter = new KeyValue[] { new KeyValue("USER_NAME", userName) };
            resultCode = imsapi.mdataGetUserData(sessionContext, StationNumber, mdataGetUserDataFilter, mdataGetUserDataKeys, out mdataGetUserDataValues, out hasMore);
            LogHelper.Info("Api mdataGetUserData user name =" + userName + " ,result code =" + resultCode);
            if (resultCode == 0)
            {
                teamNo = mdataGetUserDataValues[0];
            }
            return teamNo; ;
        }
    }
}
