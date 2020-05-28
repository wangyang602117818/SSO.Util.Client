# SSO.Client 使用文档
该工具集成在 .NetFramework 的 Web 项目中.<br>
会自动安装以下依赖: <br>
Newtonsoft.Json<br>
Microsoft.AspNet.Mvc<br>
log4net<br>
System.IdentityModel.Tokens.Jwt
## 第一步: 安装 和 配置
```
nuget上搜索 SSO.Util.Client 关键词安装
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

### 第二步: SSO验证
```
[SSOAuthorize]  :需要登录才能访问
[SSOAuthorize(Roles = "admin")] :需要相应的role才能访问
[AllowAnonymous] :可以匿名访问
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
### 第三步: 日志记录
日志的配置文件已经作为资源文件在 SSO.Util.Client.dll 中(SSO.Util.Client.log4net.config)
```
//在应用程序启动的时候添加 
var assembly = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + "bin\\SSO.Util.Client.dll");
var stream = assembly.GetManifestResourceStream("SSO.Util.Client.log4net.config");
log4net.Config.XmlConfigurator.Configure(stream);
//记录日志的方式
Log4Net.InfoLog("xx");
Log4Net.ErrorLog("xx");
```
### 第四步: 其他工具方法
```
var str =  AppSettings.GetValue("key")  //获取配置文件
AsymmetricEncryptHelper: 非对称加密解密类
Base64SecureURL: url安全的base64加密解密
DateTimeExtention: 获取时间戳
HashEncryptHelper: 散列算法
HttpRequestHelper: http请求方法
ImageExtention: 图片方法
JsonSerializerHelper: json序列化方法
Log4Net:日志记录方法
MsQueue:消息队列方法
RandomExtention:随机数
StreamExtention: 流扩展(流的md5,sha256)
StringExtention: 字符串扩展(md5,sha256)
SymmetricEncryptHelper: 对称加密算法
JwtManager: JwtToken 生成类
```