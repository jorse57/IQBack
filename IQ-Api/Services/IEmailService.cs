using Microsoft.AspNetCore.Mvc;

namespace IQ_Api.Services
{
    public interface IEmailService
    {
        ActionResult<string> Send(string to, string subject);
    }
}
