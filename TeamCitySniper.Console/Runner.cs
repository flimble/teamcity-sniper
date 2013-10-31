using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using Microsoft.Exchange.WebServices.Data;
using MissileSharp;
using NDesk.Options;
using log4net;

namespace TeamCitySniper.Console
{
    public class Runner
    {
        private static readonly ILog Logger =LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public void Run(string[] args)
        {            

            Logger.Debug(String.Join(",", args));




            bool helpRequested = false;

            string username = null, password = null, autodiscoverSmtpAddress = null, domain = null;
            string outdir = null;
            string minMailReceivedDate = null;            



            var p = new OptionSet() {
                                                
                    {"help|?", "show this message and exit", h => helpRequested = (h != null) },     
                    {"calibrate", c => Calibrate()},
                    {"autodiscoverSmtpAddress=", "email address used to autodiscover exchange web service url",  m => autodiscoverSmtpAddress = m},
                    {"username=", "Optional network user. If none provided, running user will be used.",  u => username = u},
                    {"password=", "Optional network password",  pwd => password = pwd},
                    {"domain=", "Optional domain name",  d => domain = d},
                    {"minMailReceivedDate=", "Optional minimum mail received date. This stops processing ALL messages", d=> minMailReceivedDate = d }
            };



            p.Parse(args);

            if (helpRequested)
                ShowHelp(p);




            Logger.DebugFormat("username={0}", username);
            Logger.DebugFormat("domain={0}", domain);
            Logger.DebugFormat("password={0}", password);
            Logger.DebugFormat("minMailReceivedDate={0}", minMailReceivedDate);
            Logger.DebugFormat("autodiscoverSmtpAddress={0}", autodiscoverSmtpAddress);


            if (string.IsNullOrEmpty(autodiscoverSmtpAddress))
                autodiscoverSmtpAddress =
                    ConfigurationManager.AppSettings["AutoDiscoverExchangeSettingsFromThisMailAddress"];

            if (String.IsNullOrEmpty(autodiscoverSmtpAddress))
                throw new ArgumentNullException("autodiscoverSmtpAddress");





            DateTime? minDate = null;
            if (!String.IsNullOrEmpty(minMailReceivedDate))
            {
                DateTime d;
                bool success = DateTime.TryParseExact(minMailReceivedDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);

                if (!success)
                    throw new ArgumentException("Minimum date was provided but was not in the valid format 'yyyymmdd' e.g. '20120123'");

                minDate = d;
            }


            var exchangeService = new ExchangeService(ExchangeVersion.Exchange2010_SP2);



            if (username != null)
            {
                if (password == null)
                    throw new ArgumentNullException(password);

                if (domain == null)
                    throw new ArgumentNullException(domain);

                Logger.DebugFormat("Impersonating user: {0} in domain: {1}", username, domain);

                ImpersonateUser(username, domain, password);
            }


            Logger.DebugFormat("Auto-discovering url using email: {0}", autodiscoverSmtpAddress);

            exchangeService.AutodiscoverUrl(autodiscoverSmtpAddress, redirect => true);


            IProcessMailItem processor = new ProcessFailureMessage(exchangeService, new CommandCenter(new ThunderMissileLauncher()), new FileReader(), ConfigurationManager.AppSettings["SuspectsConfigFile"]);


            IFindMailItems finder = new TeamCityMailerFinder(exchangeService, 0, minDate);

            var items = finder.FindItems();

            if (items.Count == 0)
            {
                Logger.Debug("No Failures Found");
                return;
            }


            foreach (var email in items)
            {
                processor.Process(email);
            }
        }

        private void ShowHelp(OptionSet p)
        {
            p.WriteOptionDescriptions(System.Console.Out);
        }

        private void ImpersonateUser(string username, string domain, string password)
        {
            IntPtr tokenHandle = new IntPtr(0);

            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token.
            const int LOGON32_LOGON_INTERACTIVE = 2;

            tokenHandle = IntPtr.Zero;

            // Call LogonUser to obtain a handle to an access token.
            bool loggedOn = LogonUser(username, domain, password,
                LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                ref tokenHandle);

            if (!loggedOn)
                throw new SecurityException(string.Format("Attempting impersonation but unable to log on as user {0} in domain {1}", username, domain));

            WindowsIdentity windowsIdentity = new WindowsIdentity(tokenHandle);


            WindowsImpersonationContext impersonationContext =
            windowsIdentity.Impersonate();
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername,
            String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);


        public static void Calibrate()
        {
            var reader = ConfigReader.Read(ConfigurationManager.AppSettings["SuspectsConfigFile"]);

            System.Console.WriteLine("Current Suspects: {0}", string.Join(",", reader.GetCommandSetNames()));
            System.Console.WriteLine("Select an action to calibrate");
            System.Console.WriteLine("possible actions include: reset, right:10, left:10, up:10, down:10, fire");
            //Console.WriteLine(
            //    "type in a suspect name to create new, or type an existing name above to edit configuration settings");
            using (var launcher = new CommandCenter(new ThunderMissileLauncher()))
            {



                while (true)
                {
                    string action = System.Console.ReadLine();

                    if (action == "exit")
                        break;



                    var directions = new[] { "right", "left", "up", "down" };

                    if (action == "fire")
                    {
                        System.Console.WriteLine("Fire Missile");
                        launcher.Fire(1);
                    }
                    else if (action == "reset")
                    {
                        System.Console.WriteLine("Resetting Launcher");
                        launcher.Reset();
                    }
                    if (directions.Any(direction => action.Contains(direction)))
                    {


                        var commands = action.Split(':');
                        var command = commands[0];
                        var value = int.Parse(commands[1]);

                        System.Console.WriteLine("Running command: {0} with value: {1}", command, value);
                        launcher.RunCommand(new LauncherCommand(command, value));
                    }

                }
            }
        }
    }


}
