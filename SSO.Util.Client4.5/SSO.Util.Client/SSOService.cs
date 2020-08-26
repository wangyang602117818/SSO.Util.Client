using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SSO.Util.Client
{
    /// <summary>
    /// sso服务类
    /// </summary>
    public class SSOService
    {
        private static string baseUrl = AppSettings.GetValue("ssoBaseUrl");
        private HttpRequestHelper requestHelper = new HttpRequestHelper();
        /// <summary>
        /// 获取所有company
        /// </summary>
        /// <returns></returns>
        public ServiceModel<List<CompanyItem>> GetAllCompany()
        {
            string companys = requestHelper.Get(baseUrl.TrimEnd('/') + "/sso/getallcompany", null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<CompanyItem>>>(companys);
        }
        /// <summary>
        /// 获取指定department列表
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public ServiceModel<List<DepartmentItem>> GetAllDepartment(string companyCode)
        {
            string departments = requestHelper.Get(baseUrl.TrimEnd('/') + "/sso/getalldepartment/" + companyCode, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<DepartmentItem>>>(departments);
        }
        /// <summary>
        /// 获取user列表
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="companyCode"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ServiceModel<List<UserItem>> GetUserList(string companyCode = "", string filter = "", int pageIndex = 1, int pageSize = 10)
        {
            string users = requestHelper.Get(baseUrl.TrimEnd('/') + "/sso/getuserlist?companyCode=" + companyCode + "&filter=" + filter + "&pageIndex=" + pageIndex + "&pageSize=" + pageSize, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<UserItem>>>(users);
        }
        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ServiceModel<List<RoleItem>> GetRoleList(string filter = "", int pageIndex = 1, int pageSize = 10)
        {
            string roles = requestHelper.Get(baseUrl.TrimEnd('/') + "/sso/getrolelist?filter=" + filter + "&pageIndex=" + pageIndex + "&pageSize=" + pageSize, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<RoleItem>>>(roles);
        }
        /// <summary>
        /// 获取user详情
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ServiceModel<UserDetail> GetUserDetail(string userId)
        {
            string user = requestHelper.Get(baseUrl.TrimEnd('/') + "/sso/getuser/" + userId, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<UserDetail>>(user);
        }
    }
    /// <summary>
    /// Company类
    /// </summary>
    public class CompanyItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }
    /// <summary>
    /// department类
    /// </summary>
    public class DepartmentItem
    {
        [JsonProperty("value")]
        public string Code { get; set; }
        [JsonProperty("title")]
        public string Name { get; set; }
        public int Order { get; set; }
        public IEnumerable<DepartmentItem> Children { get; set; }
    }
    /// <summary>
    /// User列表类
    /// </summary>
    public class UserItem
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string CompanyCode { get; set; }
        public string Sex { get; set; }
        public string Mobile { get; set; }
        public string CompanyName { get; set; }
        public string DepartmentName { get; set; }
        public string RoleName { get; set; }
        public DateTime? CreateTime { get; set; }
    }
    /// <summary>
    /// 用户详情类
    /// </summary>
    public class UserDetail
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string IdCard { get; set; }
        public string Sex { get; set; }
        public bool IsModified { get; set; }
        public string[] DepartmentCode { get; set; }
        public string[] DepartmentName { get; set; }
        public string[] Role { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
    /// <summary>
    /// 角色列表项
    /// </summary>
    public class RoleItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
