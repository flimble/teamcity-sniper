using System.Collections.Generic;

namespace TeamCitySniper
{
    public interface IFindMailItems
    {
        IList<EmailMessage> FindItems();
    }
}