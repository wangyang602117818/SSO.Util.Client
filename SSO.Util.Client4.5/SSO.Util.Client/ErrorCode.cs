﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    public enum ErrorCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        success = 0,
        /// <summary>
        /// 重定向
        /// </summary>
        redirect = 1,

        authcode_is_null = 101,
        app_not_exist = 102,
        app_blocked = 103,
        error_permission = 104,
        invalid_params = 105,
        params_valid_fault = 106,
        owner_not_match = 107,
        file_type_blocked = 108,
        username_required = 109,
        usercode_required = 110,
        no_handler_available = 111,
        api_not_available = 112,
        url_not_available = 113,
        old_password_error = 114,
        /// <summary>
        /// 不存在
        /// </summary>
        record_not_exist = 200,
        uri_not_found = 201,
        usercode_exist = 202,
        login_fault = 203,
        task_not_completed = 204,
        invalid_password = 205,
        file_type_not_match = 206,
        /// <summary>
        /// 请求已经超过次数
        /// </summary>
        requests_has_been_exceeded = 220,
        /// <summary>
        /// 已存在
        /// </summary>
        record_exist = 400,
        /// <summary>
        /// 权限不足
        /// </summary>
        authorize_fault = 401,
        invalid_token = 402,
        token_expired = 403,

        /// <summary>
        /// 任务未完成
        /// </summary>
        task_not_complete = 300,
        /// <summary>
        /// 配置文件缺失
        /// </summary>
        baseUrl_not_config = 600,
        secretKey_not_config = 601,
        cookieKey_not_config = 602,
        cookieTime_not_config = 603,
        messageBaseUrl_not_config = 604,
        /// <summary>
        /// 记录已被使用
        /// </summary>
        record_has_been_used = 700,
        task_is_running = 701,
        /// <summary>
        /// 未知错误
        /// </summary>
        server_exception = -1000
    }
}
