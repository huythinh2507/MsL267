using Newtonsoft.Json;
using System.Net;

namespace PresentationLayer.Controllers
{
    public class ExceptionHandlingMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Handle the exception and return a response
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new
                {
                    context.Response.StatusCode,
                    Message = "An unexpected error occurred.",
                    Detail = ex.Message
                };

                await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
            }
        }
    }

}
