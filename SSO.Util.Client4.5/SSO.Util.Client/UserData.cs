using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    public class UserData
    {
        public string UserId = null;
        public string UserName = null;
        public string Lang = null;
        public IEnumerable<string> UserRoles = null;
        public string Company = null;
        public IEnumerable<string> Departments = null;
    }
}
