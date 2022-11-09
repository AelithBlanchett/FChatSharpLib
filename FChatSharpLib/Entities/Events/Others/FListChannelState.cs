using System.Collections.Generic;

namespace FChatSharpLib.Entities.Events.Helpers
{
    public class FListChannelState
    {
        public string name;
        public int characters;
        public string title;
        public string mode;

        public new string ToString()
        {
            return Title;
        }

        public string Title
        {
            get
            {
                return IsOfficial ? name : title;
            }
        }

        public bool IsOfficial
        {
            get
            {
                return string.IsNullOrEmpty(title);
            }
        }
    }
}
