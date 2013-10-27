using System;

namespace TeamCitySniper
{
    public interface IProcessMailItem
    {
        void Process(EmailMessage mailItem);
    }
}