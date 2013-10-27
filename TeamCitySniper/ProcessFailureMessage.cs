using System;
using System.Linq;
using System.Reflection;
using Microsoft.Exchange.WebServices.Data;
using MissileSharp;
using log4net;

namespace TeamCitySniper
{
    public class ProcessFailureMessage : IProcessMailItem
    {
        private readonly ExchangeService _exchangeService;
        private readonly ICommandCenter _commandCenter;
        private readonly string _settingsFile;
        

        public ProcessFailureMessage(ExchangeService exchangeService, ICommandCenter commandCenter, string settingsFile)
        {
            _exchangeService = exchangeService;
            _commandCenter = commandCenter;
            _settingsFile = settingsFile;
        }

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Process(EmailMessage mailItem)
        {            
            Logger.DebugFormat("Bound mail item received on:{0} with subject:{1}", mailItem.DateTimeReceived, mailItem.Subject);            
            
            //parse the email and call teamcity           
            var sniper = new MissileLauncher(_commandCenter,_settingsFile);
            
            foreach (var suspect in sniper.Suspects)
            {
                if (mailItem.Body.Contains(suspect))
                {
                    sniper.Execute(suspect);
                    Logger.DebugFormat("Fired at {0}", suspect);
                }
            }           
            MarkProcessingComplete(mailItem);
        }


        

        private void MarkProcessingComplete(EmailMessage mailItem)
        {
            var item = Item.Bind(_exchangeService, new ItemId(mailItem.UniqueIdentifier));

            Logger.Debug("Processing complete. Copying mail item to processed folder");

            const string folderName = "Processed";
            var processedFolderSearch = new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folderName);

            try
            {
                var destinationFolder = _exchangeService.FindFolders(WellKnownFolderName.Inbox, processedFolderSearch, new FolderView(1)).Single();
                item.Move(destinationFolder.Id);
            }
            catch (FormatException e)
            {
                throw new ApplicationException(string.Format("Cannot find the following folder: {0} in the inbox", folderName));
            }

        }

     
    }
}
