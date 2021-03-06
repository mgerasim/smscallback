﻿using System;
using Limilabs.Client.SMTP;
using Limilabs.Mail;
using Limilabs.Mail.Fluent;
using Limilabs.Mail.Headers;

namespace SmtpSend
{
    class Program
    {
        private const string _server = "smtp.company.com";
        private const string _user = "user";
        private const string _password = "password";

        static void Main()
        {
            IMail email = Mail
                .Html(@"<img src=""cid:lemon@id"" align=""left"" /> This is simple 
                        <strong>HTML email</strong> with an image and attachment")
                .Subject("Subject")
                .AddVisual("Lemon.jpg").SetContentId("lemon@id")
                .AddAttachment("Attachment.txt").SetFileName("Invoice.txt")
                .From(new MailBox("orders@company.com", "Lemon Ltd"))
                .To(new MailBox("john.smith@gmail.com", "John Smith"))
                .Create();

            email.Save(@"SampleEmail.eml");             // You can save the email for preview.

            using (Smtp smtp = new Smtp())              // Now connect to SMTP server and send it
            {
                smtp.Connect(_server);                  // Use overloads or ConnectSSL if you need to specify different port or SSL.
                smtp.UseBestLogin(_user, _password);    // You can also use: Login, LoginPLAIN, LoginCRAM, LoginDIGEST, LoginOAUTH methods,
                                                        // or use UseBestLogin method if you want Mail.dll to choose for you.
                
                SendMessageResult result = smtp.SendMessage(email);
                Console.WriteLine(result.Status);

                smtp.Close();
            }

            // For sure you'll need to send complex emails, 
            // take a look at our templates support in SmtpTemplates sample.
        }
    };
}
