using System.Net;
using Hotel.Domain.DTOs;
using Hotel.Domain.Exceptions;

namespace Hotel.Domain.Middlewares;

public class HandleExceptionMiddleware 
{
  private readonly RequestDelegate _next;
  public HandleExceptionMiddleware(RequestDelegate next)
  =>  _next = next;
  
  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch(ValidationException e)
    {
      context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      await context.Response.WriteAsJsonAsync(
        new Response<string>(400,e.Message)
      );
    }
    catch(ArgumentException e)
    {
      context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      await context.Response.WriteAsJsonAsync(
        new Response<string>(400,e.Message)
      );
    }
  }

}