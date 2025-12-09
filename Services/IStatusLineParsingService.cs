using CommonLogic.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stand7
{
    public interface IStatusLineParsingService
    {
        string ParseStatusLine(SensorReading statusLine);
    }
}
