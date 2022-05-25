using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Text;
using System.Threading.Tasks;

namespace CastleBook.Utility
{
    public class EmailSender : IEmailSender
    {


        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
           /* var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse("hello@gmail.com"));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate("castle.bookk@gmail.com", "Castlebook123");
                emailClient.Send(emailToSend);
                emailClient.Disconnect(true);
            }*/
            return Task.CompletedTask;

        }
        
    }
}
