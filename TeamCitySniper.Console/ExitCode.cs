using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCitySniper.Console
{
    public enum ExitCode
    {
        Success = 0,
        Fail = 1,
        InvalidInput = 2,
        CannotConnectToLauncher = 3,
        CannotFindSettings = 4,
        CannotConnectToMailServer = 5
    }
}
