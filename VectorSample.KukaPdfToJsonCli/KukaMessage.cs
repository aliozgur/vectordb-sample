using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorSample.KukaPdfToJsonCli
{
    public class KukaMessage
    {
        public int Code { get; set; }
        public string Title { get; set; }
        public string Cause { get; set; }
        public string Effect { get; set; }
        public string Remedy { get; set; }
    }

}
