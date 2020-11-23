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
### 安装 和 配置
1. nuget上搜索 SSO.Util.Client 关键词安装
2. 在配置文件中配置好节点
3. 编译错误处理: 如果出现了 Could not load file or assembly 的错误,请更新相应的程序集到最新版本
### 功能一: SSO验证
- `[SSOAuthorize]`  : 需要登录才能访问
- `[SSOAuthorize("GetFileDetail")]` : 到数据库查询是否有 GetFileDetail 权限
- `[AllowAnonymous]` :可以匿名访问
- 把所有标记了 `SSOAuthorize("name")` 的 name部分插入数据库,用来在界面配置权限
   ```
      var assembly = Assembly.GetExecutingAssembly();
      var controllers = assembly.GetTypes().Where(w => w.FullName.Contains("FileService.Api.Controllers"));
      var res = SSOAuthorizeAttribute.GetPermissionDescription(controllers);
      SSOClientService sSOClientService = new SSOClientService(ssoBaseUrl, JwtManager.GetAuthorization(Request));
      var category = AppSettings.GetApplicationUrlTrimHttpPrefix(Request);
      var result = sSOClientService.ReplacePermissions(category, res);
   ```
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
### 功能二: 日志记录
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
### 功能三: 验证和返回值
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
### 功能四: ORM操作sqlserver数据库
1. 在项目的根目录下建立 sbl.config.xml 配置文件,设置成 Copy always
   ```
   <?xml version="1.0" encoding="utf-8" ?>
   <configuration>
      <connectionstring>
         Server=.;Database=T01;User ID=name;Password=123
      </connectionstring>
      <!--创建table-->
      <create-tables resource="create.sbl.xml" assembly="SSO.Data"/>
      <mappings assembly="SSO.Data" namespace="Mappings"/>
   </configuration>
   ```
2. 在项目的合适的位置添加 create.sbl.xml,设置成 Embedded Resource (可以忽略)
   ```
   <?xml version="1.0" encoding="utf-8" ?>
   <sql>
      if not exists(SELECT * FROM information_schema.TABLES WHERE table_name ='Company')
      create table Company (
      Id INT IDENTITY NOT NULL,
      Code NVARCHAR(30) not null,
      Name NVARCHAR(50) not null,
      Description NVARCHAR(512) null,
      [Order] INT null,
      UpdateTime DATETIME null,
      CreateTime DATETIME null,
      primary key (Id),
      unique (Code)
      )
   </sql>
   ```
3. 在项目合适的位置添加 mappings 映射文件(company.sbl.xml),设置成 Embedded Resource
   ```
   <?xml version="1.0" encoding="utf-8" ?>
   <sql>
      <insert>
         INSERT INTO Company (
         Code,
         Name,
         [Order],
         CreateTime
         <isNotEmpty property="Description" prepend=",">
            Description
         </isNotEmpty>
         ) VALUES (
         @Code,
         @Name,
         @Order,
         @CreateTime
         <isNotEmpty property="Description" prepend=",">
            @Description
         </isNotEmpty>
         )
      </insert>
      <update>
         UPDATE Company SET
         Code = @Code,
         Name=@Name,
         UpdateTime=@UpdateTime,
         [Order]=@Order
         <isNotEmpty property="Description" prepend=",">
            Description=@Description
         </isNotEmpty>
         WHERE Id = @Id
      </update>
      <delete>
         delete from Company where Id in (<iterate conjunction="," property="Ids">@Ids{{index}}</iterate>)
      </delete>
      <get-by-id>
         select * from Company where Id = @Id
      </get-by-id>
      <get-page-list>
         select * from(
         select *,row_number() over(order by Id desc) uid,(select count(1) from [Company]) total from [Company]
         <isNotEmpty property="Name" prepend="where">
            Name like '%'+@Name+'%'
         </isNotEmpty>
         ) as tbl
         where uid between (@PageIndex-1)*@PageSize+1 and @PageIndex*@PageSize
      </get-page-list>
   </sql>
   ```
4. 新建数据表访问基类和数据访问类
   ```
   public abstract class ModelBase : EntityBase
   {
        static SessionFactory sessionFactory = null;
        static ModelBase()
        {
            sessionFactory = new Configuration().Configure();
        }
        public List<T> GetPageList<T>(ref int count, object t, object replacement) where T : class
        {
            return QueryList<T>("get-page-list", t, replacement, ref count);
        }
   }
   ```
   sessionFactory是单例模式，在调用Configure()方法时，默认读取 sbl.config.xml 配置文件:
   1. 找到链接字符串 connectionstring
   2. 找到指定的 create.sbl.xml，运行里面的sql语句创建数据表
   3. 找到所有 mappings 映射文件,缓存起来
   ```
   public class Company : ModelBase
   {
      public Company() { }
      public int Id { get; set; }
      public string Code { get; set; }
      public string Name { get; set; }
      public string Description { get; set; }
      public int Order { get; set; }
      public DateTime? UpdateTime { get; set; }
      public DateTime? CreateTime { get; set; }

      public T GetById<T>(int id) where T : class
      {
         return QueryObject<T>("get-by-id", new { Id = id }, null);
      }
   }
   ```
   Company类和company.sbl.xml配置文件对应,PermissionRoleMapping类则对应permission_role_mapping.sbl.xml配置文件  
   Company类具有以下能力:
   1. 数据插入: new Company().Insert(obj)   //自动查找company.sbl.xml中的insert节点
   2. 数据修改: new Company().Update(obj)   //自动查找company.sbl.xml中的update节点
   3. 数据珊瑚: new Company().Delete(new { Ids = ids })  //自动查找company.sbl.xml中的delete节点
   4. 根据id查找: new Company().GetById(id) //自动查找company.sbl.xml中的get-by-id节点
   5. 分页查找: new Company().GetPageList(id) //自动查找company.sbl.xml中的get-page-list节点
5. 动态sql  
   `isNotEmpty` 节点:  (`property`: 当指定的属性不为null并且不为""时 , 添加该节点, `prepend`: 添加节点时语句前面添加该字符)  
   `isNotNull` 节点:  (`property`: 当指定的属性不为null时,添加该节点 ,  `prepend`: 添加节点时语句前面添加该字符 )  
   `iterate` 迭代节点: (`property`: 迭代的属性, `conjunction`: 迭代的语句用该符号连接), 每一次迭代都会把语句中 {{index}} 替换成元素下标
   
   
   
### 其他工具方法  
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