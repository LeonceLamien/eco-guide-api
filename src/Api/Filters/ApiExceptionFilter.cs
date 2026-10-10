using EcoGuide.Data.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EcoGuide.Api.Filters;

/// <summary>
/// Intercept and process exceptions.
/// </summary>
public class ApiExceptionFilter : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        var exception = context.Exception;

        // Return 404 if exception is of type DocumentNotFoundException
        if (exception is DocumentNotFoundException)
        {
            context.Result = new NotFoundObjectResult(exception.Message);

            context.ExceptionHandled = true;
        }
    }
}