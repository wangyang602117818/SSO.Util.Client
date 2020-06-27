using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    public class HttpRequestState
    {
        public HttpWebRequest request = null;
        public HttpWebResponse response = null;
        public Stream responseStream = null;
    }
}
