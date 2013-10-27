using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Exchange.WebServices.Data;
using log4net;

namespace TeamCitySniper
{
    public class TeamCityMailerFinder : IFindMailItems
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ExchangeService _exchangeService;
        private int _maxMessagesToProcess;
        private readonly DateTime? _processFromDate;
        

        public TeamCityMailerFinder(ExchangeService exchangeService, int maxMessagesToProcess, DateTime? processFromDate)
        {
            _exchangeService = exchangeService;
            _maxMessagesToProcess = maxMessagesToProcess;
            _processFromDate = processFromDate;

             ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
        }

      

        public IList<EmailMessage> FindItems()
        {            
            IList<EmailMessage> results = new List<EmailMessage>();

            if (_maxMessagesToProcess == 0)
                _maxMessagesToProcess = 1000;

            Logger.DebugFormat("Finding all messages in inbox with attachments {0}", _processFromDate);

            var view = new ItemView(_maxMessagesToProcess);
            view.Traversal = ItemTraversal.Shallow;

            var isEmailWithAttachments = new SearchFilter.SearchFilterCollection(LogicalOperator.And)
                {
                    new SearchFilter.ContainsSubstring(ItemSchema.Subject, "[TeamCity, FAILED]"),                                        
                    new SearchFilter.IsEqualTo(ItemSchema.ItemClass, "IPM.Note"),                    
                };

            if (_processFromDate.HasValue)
                isEmailWithAttachments.Add(new SearchFilter.IsGreaterThan(ItemSchema.DateTimeReceived, _processFromDate));

            var mailItems = _exchangeService.FindItems(WellKnownFolderName.Inbox, isEmailWithAttachments, view).ToList();

            Logger.DebugFormat("Retrieved: {0} missed messages on startup to process", mailItems.Count());

            foreach (var item in mailItems)
            {
                var message = Item.Bind(_exchangeService, item.Id);

                var email = new EmailMessage
                    {
                        UniqueIdentifier = message.Id.UniqueId,
                        Body = message.Body.Text,
                        Recipient = message.DisplayTo,
                        DateTimeReceived = message.DateTimeReceived,
                        DateTimeSent = message.DateTimeSent,
                        Subject = message.Subject
                    };
                results.Add(email);
            }

            return results;
        } 
    }
}
