using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SSO.Util.Client
{
    /// <summary>
    /// sso服务类
    /// </summary>
    public class SSOClientService
    {
        private HttpRequestHelper requestHelper = new HttpRequestHelper();
        /// <summary>
        /// sso服务的url
        /// </summary>
        public string RemoteUrl { get; set; }
        /// <summary>
        /// jwt token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 请求header
        /// </summary>
        Dictionary<string, string> headers = new Dictionary<string, string>();
        /// <summary>
        /// sso服务的url
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <param name="token"></param>
        public SSOClientService(string remoteUrl, string token)
        {
            RemoteUrl = remoteUrl.TrimEnd('/');
            Token = token;
            headers.Add("Authorization", token);
        }
        /// <summary>
        /// 获取所有company
        /// </summary>
        /// <returns></returns>
        public ServiceModel<List<CompanyItem>> GetAllCompany()
        {
            string companys = requestHelper.Get(RemoteUrl + "/company/getall", headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<CompanyItem>>>(companys);
        }
        /// <summary>
        /// 获取指定department列表
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public ServiceModel<List<DepartmentItem>> GetAllDepartment(string companyCode)
        {
            string departments = requestHelper.Get(RemoteUrl + "/department/getDepartments?companyCode=" + companyCode, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<DepartmentItem>>>(departments);
        }
        /// <summary>
        /// 获取user列表
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="companyCode"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderField">排序名称</param>
        /// <param name="orderType">排序规则:asc desc</param>
        /// <returns></returns>
        public ServiceModel<List<UserItem>> GetUserList(string companyCode = "", string filter = "", int pageIndex = 1, int pageSize = 10, string orderField = "UserName", string orderType = "asc")
        {
            string users = requestHelper.Get(RemoteUrl + "/user/getBasic?companyCode=" + companyCode + "&filter=" + filter + "&orderField=" + orderField + "&orderType=" + orderType + "&pageIndex=" + pageIndex + "&pageSize=" + pageSize, headers);
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
            string roles = requestHelper.Get(RemoteUrl + "/role/getlist?filter=" + filter + "&pageIndex=" + pageIndex + "&pageSize=" + pageSize, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<RoleItem>>>(roles);
        }
        /// <summary>
        /// 获取user详情
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ServiceModel<UserDetail> GetUserDetail(string userId)
        {
            string user = requestHelper.Get(RemoteUrl + "/user/getByUserId?userId=" + userId, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<UserDetail>>(user);
        }
        /// <summary>
        /// 获取当前用户所有权限
        /// </summary>
        /// <returns></returns>
        public ServiceModel<IEnumerable<string>> GetUserPermissions()
        {
            string permission = requestHelper.Get(RemoteUrl + "/user/getPermissions", headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<IEnumerable<string>>>(permission);
        }
        /// <summary>
        /// 替换权限项
        /// </summary>
        /// <param name="origin">项目标记</param>
        /// <param name="names">权限列表</param>
        /// <returns></returns>
        public ServiceModel<string> ReplacePermissions(string origin, IEnumerable<string> names)
        {
            string result = requestHelper.Post(RemoteUrl + "/permission/add", new { Origin = origin, Names = names }, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
    }
    /// <summary>
    /// Company类
    /// </summary>
    public class CompanyItem
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
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
        public string Description { get; set; }
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
