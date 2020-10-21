using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SSO.Util.Client
{
    /// <summary>
    /// 权限描述
    /// </summary>
    public class PermissionDescriptionAttribute : Attribute
    {
        /// <summary>
        /// 权限的名称
        /// </summary>
        public string PermissionName = "";
        /// <summary>
        /// 权限描述
        /// </summary>
        /// <param name="permissionName">权限名称</param>
        public PermissionDescriptionAttribute(string permissionName)
        {
            PermissionName = permissionName;
        }
        /// <summary>
        /// 获取程序集所有带有 PermissionDescriptionAttribute 的方法
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static List<string> GetPermissionDescription(IEnumerable<Type> types)
        {
            List<string> actions = new List<string>();
            foreach (var item in types)
            {
                var methods = item.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(PermissionDescriptionAttribute));
                    foreach (Attribute att in attributes)
                    {
                        var name = ((PermissionDescriptionAttribute)att).PermissionName;
                        if (!actions.Contains(name)) actions.Add(name);
                    }
                }
            }
            return actions;
        }
    }
}
