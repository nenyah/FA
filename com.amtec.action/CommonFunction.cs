using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using FA_COATING.com.amtec.forms;
namespace com.amtec.action
{
    public class CommonFunction
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private MainFrom view;

        public CommonFunction(IMSApiSessionContextStruct sessionContext, MainFrom view)
        {
            this.sessionContext = sessionContext;
            this.view = view;
        }

        public string GetSiteExtNoBySiteNo(string siteNo)
        {
            int error = 0;
            string siteExtNo = siteNo;
            KeyValue[] machineAssetStructureFilter = { new KeyValue("SITE_NUMBER", siteNo) };
            string[] machineAssetStructureResultKeys = { "SITE_NUMBER_EXT" };
            string[] machineAssetStructureValues = { };
            error = imsapi.mdataGetMachineAssetStructure(sessionContext, view.config.StationNumber, machineAssetStructureFilter, machineAssetStructureResultKeys, out machineAssetStructureValues);
            LogHelper.Info("Api mdataGetMachineAssetStructure site no =" + siteNo + " , result code =" + error);
            if (error == 0)
                siteExtNo = machineAssetStructureValues[0];
            LogHelper.Info("Site ext no =" + siteExtNo);
            return siteExtNo;
        }

        public string GetSiteNoByStationNo(string stationNumber)
        {
            int error = 0;
            string siteNo = "";
            KeyValue[] machineAssetStructureFilter = { new KeyValue("STATION_NUMBER", stationNumber) };
            string[] machineAssetStructureResultKeys = { "SITE_NUMBER" };
            error = imsapi.mdataGetMachineAssetStructure(sessionContext, stationNumber, machineAssetStructureFilter, machineAssetStructureResultKeys, out string[] machineAssetStructureValues);
            LogHelper.Info("Api mdataGetMachineAssetStructure station number =" + siteNo + " , result code =" + error);
            if (error == 0)
                siteNo = machineAssetStructureValues[0];
            LogHelper.Debug("Site no =" + siteNo);
            return siteNo;
        }
    }
}
