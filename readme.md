# SSO.Client 使用文档
该工具集成在 .NetFramework 的 Web 项目中.<br>
会自动安装以下依赖: <br>
System.Configuration<br>
System.Web<br>
Microsoft.AspNet.Mvc<br>
Newtonsoft.Json<br>
System.IdentityModel.Tokens.Jwt
## 第一步: 安装 和 配置
```
nuget上搜索 Mvc.SSO.Client 关键词安装
并在 web.config 中配置一下4个节点
//项目的基本地址
<add key="ssoBaseUrl" value=""/>
//解密秘钥,要和基本地址的秘钥一致
<add key="ssoSecretKey" value=""/>
//cookie的键名称
<add key="ssoCookieKey" value="c.web.auth"/>
//cookie保存的时间
<add key="ssoCookieTime" value="session"/>
```
### 错误处理

如果出现了 Could not load file or assembly 的错误,请更新相应的程序集到最新版本

### 使用
```
[SSOAuthorize]  :需要登录才能访问
[SSOAuthorize(Roles = "admin")] :需要相应的role才能访问
[AllowAnonymous] :可以匿名访问
```
### 数据
```
//访问用户id
var userId = User.Identity.Name; //或者
var userId = SSOAuthorizeAttribute.UserData.UserId;
//访问用户名称 
var userName = SSOAuthorizeAttribute.UserData.UserName;
//访问角色
var roles = SSOAuthorizeAttribute.UserData.UserRoles;
//访问公司
var company = SSOAuthorizeAttribute.UserData.Company;
//访问部门
var dept = SSOAuthorizeAttribute.UserData.Departments;
```

