
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Tree;
using MimeKit.Text;
using MimeKit;
using System.Net;
using MailKit.Net.Smtp;
using System.Reflection;

namespace IQ_Api.Services
{
    public class EmailService 
    {
        public SmtpClient _smtpClient;

        public EmailService( SmtpClient smtpClient)
        {
            _smtpClient = smtpClient;
          
        }

        public ActionResult<string> Send(string to, string subject)
        {
            try
            {
                var resp = "Email enviado correctamente, verificar correo electronico";
                // create message
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("jhernandez@saludelectronica.com"));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = "<h1>CORREO DE PRUEBA</h1>" };

                // send email
                using var smtp = new SmtpClient();
                smtp.Connect("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("jhernandez@saludelectronica.com", "Stuntlomejor159");
                smtp.Send(email);
                smtp.Disconnect(true);

                return resp;

            }catch (Exception ex)
            {
                throw;
            }
        }


        //public async Task<ActionResult<string>> sendMail(string mailReceptor) {
        //    try
        //    {
        //        string resp = "Correo enviado correctamente"; 
        //        var smtpClient = new SmtpClient("smtp.office365.com")
        //        {
        //            Port = 587,
        //            Credentials = new NetworkCredential("jhernandez@saludelectronica.com", "Stuntlomejor159"),
        //            EnableSsl = true,

        //        };
        //        smtpClient.Send("jhernandez@saludelectronica.com", mailReceptor, "PRUEBA CORREO IQ", "ESTA ES UNA PRUEBA IQ");

        //     return resp;

        //    }catch (Exception ex) {
        //        throw;
        //    }
        //}




    }
}
