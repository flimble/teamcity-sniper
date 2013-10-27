using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCitySniper
{
    public class EmailMessage
    {
        public string UniqueIdentifier { get; set; }
        public string Recipient { get; set; }        
        public string Body { get; set; }
        public string Subject { get; set; }
        public DateTime DateTimeReceived { get; set; }
        public DateTime DateTimeSent { get; set; }
    }
}
