using Microsoft.AspNetCore.Mvc;
using TicketBurst.Contracts;

namespace TicketBurst.ServiceInfra;

public static class ApiResult
{
    public static JsonResult Error(int statusCode)
    {
        var reply = new ReplyContract<string>(
            Data: null, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = statusCode
        };
    }

    public static JsonResult Success<T>(int statusCode, T data)
    {
        var reply = new ReplyContract<T>(
            Data: data, 
            ServiceProcessMetadata.GetCombinedInfo()
        );

        return new JsonResult(reply) {
            StatusCode = statusCode
        };
    }
}
