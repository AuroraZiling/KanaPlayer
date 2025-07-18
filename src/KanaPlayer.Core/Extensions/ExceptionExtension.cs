using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace KanaPlayer.Core.Extensions;

public static class ExceptionExtension
{
    /// <summary>
    /// 将Exception转换为用户友好的消息
    /// </summary>
    /// <returns></returns>
    public static string GetFriendlyMessage(this Exception e)
    {
        switch (e)
        {
            #if NET5_0_OR_GREATER
            case HttpRequestException { StatusCode: not null } hre:
            {
                return $"HTTP请求错误 ({(int)hre.StatusCode}: {hre.StatusCode}) " + hre.StatusCode switch
                {
                    HttpStatusCode.BadRequest                                      => "请求无效，请检查输入",
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden        => "没有权限执行此操作",
                    HttpStatusCode.NotFound                                        => "请求的资源未找到",
                    HttpStatusCode.InternalServerError                             => "服务器遇到错误，请稍后再试",
                    HttpStatusCode.ServiceUnavailable                              => "服务暂时不可用，请稍后再试",
                    HttpStatusCode.GatewayTimeout or HttpStatusCode.RequestTimeout => "连接超时，请检查网络或稍后再试",
                    HttpStatusCode.BadGateway                                      => "连接错误，请检查网络或稍后再试",
                    _                                                              => "未知错误，请联系支持人员"
                };
            }            
            #else
            case HttpRequestException hre:
            {
                return "HTTP请求错误 " + hre.Message;
            }
            #endif
            case SocketException se:
            {
                return $"Socket错误 ({(int)se.SocketErrorCode}: {se.SocketErrorCode}) " + se.SocketErrorCode switch
                {
                    SocketError.HostNotFound      => "无法解析域名，请检查网络或稍后再试",
                    SocketError.ConnectionRefused => "服务器拒绝连接，请检查网络或稍后再试",
                    SocketError.TimedOut          => "连接超时，请检查网络或稍后再试",
                    _                             => "未知错误，请联系支持人员"
                };
            }
            case AggregateException ae:
            {
                var messageBuilder = new StringBuilder(e.Message);
                foreach (var innerException in ae.InnerExceptions)
                {
                    messageBuilder.Append('\n').Append(innerException.GetFriendlyMessage());
                }
                return messageBuilder.ToString();
            }
            case OperationCanceledException:
            {
                return "操作已取消";
            }
            case TimeoutException:
            {
                return "系统超时";
            }
            case JsonException:
            {
                return "无法解析JSON数据";
            }
            case FormatException:
            {
                return "格式错误";
            }
            case InvalidOperationException:
            {
                var message = e.Message;
                if (message.Contains("未找到 Play Info"))
                    return "未找到 Play Info";
                else
                    return e.Message;
            }
            default:
            {
                return e.Message;
            }
        }
    }
}