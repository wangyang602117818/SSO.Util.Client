# SSO.Client 使用文档
该工具集成在 .NetFramework 或者 .netcore 的项目中.
## 过滤器
### .netframework
```
filters.Add(new MyHandleErrorAttribute());  //全局错误处理过滤器
filters.Add(new ValidateModelStateAttribute());  //全局model验证过滤器
filters.Add(new SSOAuthorizeAttribute()); //全局sso验证过滤器
filters.Add(new LogRecordAttribute());  //全局日志记录过滤器(远程)
```
### .netcore
```
services.AddControllers(options =>
{
    options.Filters.Add(new MyHandleErrorAttribute());  //全局错误处理过滤器
    options.Filters.Add(new ValidateModelStateAttribute());   //全局model验证过滤器
    options.Filters.Add(new SSOAuthorizeAttribute());  //全局sso验证过滤器
    options.Filters.Add(new LogRecordAttribute()); //全局日志记录过滤器(远程)
});
```
### 排除过滤器
- `AllowAnonymousAttribute`:  //匿名验证过滤器
- `NoneLogRecordAttribute`:   //不记录日志过滤器
### 限流过滤器
- `ThrottlingAttribute` 
## 配置文件
### .netframework
```
<add key="ssoBaseUrl" value=""/>  <!--项目的基本地址-->
<add key="ssoSecretKey" value=""/>  <!--解密秘钥,要和基本地址的秘钥一致-->
<add key="ssoCookieKey" value="c.web.auth"/>  <!--cookie的键名称-->
<add key="ssoCookieTime" value="session"/>  <!--cookie保存的时间(分钟)-->
<add key="messageBaseUrl" value=""/>  <!--message记录地址-->
```
### .netcore
```
{
  "ssoBaseUrl": "http://www.sso.com:8030/",
  "ssoSecretKey": "",
  "ssoCookieKey": "web1.web.auth",
  "ssoCookieTime": "session",
  "messageBaseUrl": "http://www.messagecenter.com:8050/"
}
```
### 第一步: 安装 和 配置
1. nuget上搜索 SSO.Util.Client 关键词安装
2. 在配置文件中配置好节点
3. 编译错误处理: 如果出现了 Could not load file or assembly 的错误,请更新相应的程序集到最新版本
### 第二步: SSO验证
- `[SSOAuthorize]`  : 需要登录才能访问
- `[SSOAuthorize(Roles = "admin")]` : 需要相应的role才能访问
- `[AllowAnonymous]` :可以匿名访问
- 在action中访问用户id
   ```
   var userId = User.Identity.Name;
   ```
- 获取authorization
   ```
   var authorization = SSOAuthorizeAttribute.GetAuthorization(request)
   ```
- 访问用户其他信息
   ```
   var userData = SSOAuthorizeAttribute.ParseUserData(authorization)
   ```
### 第三步: 日志记录
- 把日志记录在当前项目的 App_Data\ 文件夹中(使用的是log4net)
   ```
   Log4Net.InfoLog("xx");  //详情日志
   Log4Net.ErrorLog("xx"); //异常日志
   ```
- 使用远程api记录日志(节点 `messageBaseUrl` 的地址)
   ```
   [LogRecord]:  //记录日志
   [NoneLogRecord]: //不记录日志
   ```
### 第四步: 验证和返回值
1. 注册全局错误处理器 `MyHandleErrorAttribute`
2. 注册全局model验证过滤器 `ValidateModelStateAttribute`
- `ErrorCode` : 返回值枚举
- `ResponseModel` :返回值对象(ContentResult的子类,类型为 application/json)
- 案例:
```
    return new ResponseModel<string>(ErrorCode.success, "");  //返回字符串
    return new ResponseModel<string>(ErrorCode.success, json);  //返回序列化好的json字符串
    return new ResponseModel<Object>(ErrorCode.success, obj); //返回可以序列化的对象
```
### 第五步: 其他工具方法  
> var str =  AppSettings.GetValue("key")  //获取配置文件
> `AsymmetricEncryptHelper`:   //非对称加密解密类  
> `Base64SecureURL`: //url安全的base64加密解密  
> `DateTimeExtention`: 获取时间戳  
> `HashEncryptHelper`: 散列算法  
> `HttpRequestHelper`: http请求方法  
> `ImageExtention`: 图片方法  
> `JsonSerializerHelper`: json序列化方法  
> `Log4Net`:日志记录方法  
> `MsQueue`:消息队列方法  
> `RandomExtention`:随机数  
> `StreamExtention`: 流扩展(流的md5,sha256)  
> `StringExtention`: 字符串扩展(md5,sha256)  
> `SymmetricEncryptHelper`: 对称加密算法  
> `JwtManager`: JwtToken 生成类 
> `FileClientService`: 文件服务类 