using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LiveExchangeRatesAPI.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly ILogger logger;
        private readonly RequestDelegate requestDelegate;

        public ErrorHandlerMiddleware(RequestDelegate requestDelegate, ILogger<ErrorHandlerMiddleware> logger)
        {
            this.logger = logger;
            this.requestDelegate = requestDelegate;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await requestDelegate(context);
            }
            catch (Exception ex)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                this.logger.LogError(ex?.Message);
                var result = JsonConvert.SerializeObject(new { message = ex?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}
