using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SSO.Util.Client
{
    public class ThrottlingFilterAttribute: ActionFilterAttribute
    {
        private readonly ThrottleStore store = new ThrottleStore();
        public int MaxRequests { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public ThrottlingFilterAttribute(int maxRequests, int minuts = 1)
        {
            MaxRequests = maxRequests;
            TimeSpan = TimeSpan.FromMinutes(minuts);
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string ip = filterContext.HttpContext.Request.UserHostAddress;
            ThrottleEntry entry = null;
            if (store.TryGetValue(ip, out entry))
            {
                //请求超过了1分钟
                if (entry.RequestStart + TimeSpan < DateTime.UtcNow)
                {
                    store.Rollover(ip);
                }
            }
            entry = store.IncrementRequests(ip);
            if (entry.Requests > MaxRequests)
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.requests_has_been_exceeded, "");
                return;
            }
        }
    }
    public class ThrottleStore
    {
        private readonly ConcurrentDictionary<string, ThrottleEntry> throttleStore = new ConcurrentDictionary<string, ThrottleEntry>();
        public bool TryGetValue(string key, out ThrottleEntry entry)
        {
            return throttleStore.TryGetValue(key, out entry);
        }
        public ThrottleEntry IncrementRequests(string key)
        {
            return throttleStore.AddOrUpdate(key, k =>
            {
                return new ThrottleEntry() { Requests = 1 };
            },
             (k, e) => { e.Requests++; return e; });
        }
        public void Rollover(string key)
        {
            throttleStore.TryRemove(key, out ThrottleEntry dummy);
        }
        public void Clear()
        {
            throttleStore.Clear();
        }
    }
    public class ThrottleEntry
    {
        public ThrottleEntry()
        {
            RequestStart = DateTime.UtcNow;
            Requests = 0;
        }
        public DateTime RequestStart { get; set; }
        public long Requests { get; set; }
    }
}
