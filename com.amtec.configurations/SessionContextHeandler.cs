using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.itac.mes.imsapi.domain;
using com.itac.mes.imsapi.domain.container;
using com.itac.mes.imsapi.client.dotnet;
using com.amtec.forms;
using com.amtec.action;

namespace com.amtec.configurations
{
    public class SessionContextHeandler
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionValidationStruct sessionValidationStruct;
        private IMSApiSessionContextStruct sessionContext = null;
        private int initResult;
        private LoginForm mainView;


        public SessionContextHeandler(ApplicationConfiguration config, LoginForm mainView)
        {
            this.mainView = mainView;
            sessionValidationStruct = new IMSApiSessionValidationStruct();
            sessionValidationStruct.stationNumber = config.StationNumber;
            sessionValidationStruct.stationPassword = "";
            sessionValidationStruct.user = "";
            sessionValidationStruct.password = "";
            sessionValidationStruct.client = config.Client;
            sessionValidationStruct.registrationType = config.RegistrationType;
            sessionValidationStruct.systemIdentifier = config.StationNumber;

            initResult = imsapi.imsapiInit();

            if (initResult != 0)
            {
                mainView.SetStatusLabelText("Conncection to iTAC failed", 1);
                mainView.isCanLogin = false;
                LogHelper.Info("Conncection to iTAC failed");
            }
            else
            {
                mainView.SetStatusLabelText("Conncection to iTAC established", 0);
                mainView.isCanLogin = true;
                LogHelper.Info("Conncection to iTAC established");
            }
        }

        public IMSApiSessionContextStruct getSessionContext()
        {
            if (initResult != IMSApiDotNetConstants.RES_OK)
            {
                return null;
            }
            else
            {
                int result = imsapi.regLogin(sessionValidationStruct, out sessionContext);
                if (result != IMSApiDotNetConstants.RES_OK)
                {
                    return null;
                }
                else
                {
                    return sessionContext;
                }
            }
        }
    }
}
