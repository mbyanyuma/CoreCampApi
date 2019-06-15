using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Models
{
    public class TalkModel
    {
        public string TalkTitle { get; set; }
        public string TalkAbstract { get; set; }
        public int TalkLevel { get; set; }

        public SpeakerModel Speaker { get; set; }
    }
}
