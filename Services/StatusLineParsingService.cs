using CommonLogic.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stand7
{
    public class StatusLineParsingService : IStatusLineParsingService
    {
        public string ParseStatusLine(SensorReading statusReading)
        {
            if (statusReading == null) { return "NotConnectedToPLC"; }
            switch (statusReading.Value)
            {
                case 0:
                    return "InitialTextForStatusLine";
                case 10:
                case 50:
                case 90:
                    return "StabilizationOfPressure";
                case 20:
                case 60:
                case 100:
                    return "Testing";
                case 30:
                case 70:
                case 110:
                    return "PressureRelief";
                case 40:
                    return "Status_Test1_StopedNormal";
                case 80:
                    return "Status_Test2_StopedNormal";
                case 120:
                    return "Status_Test3_StopedNormal";
                case 1101:
                    return "Status_Test1_Started";
                case 1100:
                    return "Status_Test1_WriteFailed";
                case 1000:
                    return "Status_Test_Error";
                case 1201:
                    return "Status_Test2_Started";
                case 1200:
                    return "Status_Test2_WriteFailed";
                case 1301:
                    return "Status_Test3_Started";
                case 1300:
                    return "Status_Test3_WriteFailed";
            }


            return "NotConnectedToPLC";
        }
    }
}
