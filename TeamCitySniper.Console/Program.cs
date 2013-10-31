using System;
using System.Reflection;
using log4net;
using log4net.Config;

namespace TeamCitySniper.Console
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        static int Main(string[] args)
        {            

            try
            {                
                var runner = new Runner();
                runner.Run(args);

                return (int)ExitCode.Success;
            }
            catch (Exception e )
            {
                Logger.Error(e);
                return (int)ExitCode.Fail;
            }

            

        }

        
    }
}
