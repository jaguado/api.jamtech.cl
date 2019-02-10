using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Helpers
{
    public static class Email
    {
        private static readonly string _apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        private static readonly SendGridClient _client = new SendGridClient(_apiKey);

        public static void Test()
        {
            var from = new EmailAddress("api@jamtech.cl", "JAMTech API");
            var to = new EmailAddress("jorge@jamtech.cl", "Jorge");
            var subject = "Sending with SendGrid is Fun";
            var response = Send(from, to, subject, "Test html content", "<b>Test html content</b>");
        }
        
        public static async Task<Response> Send(SendGridMessage msg)
        {
            return await _client.SendEmailAsync(msg);
        }

        public static async Task<Response> Send(EmailAddress from, EmailAddress to, string subject, string plainTextContent, string htmlContent)
        {            
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            return await Send(msg);
        }
        public static async Task<Response> SendWithAttachment(EmailAddress from, EmailAddress to, string subject, string plainTextContent, string htmlContent, string attachmentName, string attachmentBase64String)
        {
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            msg.AddAttachment(attachmentName, attachmentBase64String);
            return await Send(msg);
        }
        public static async Task<Response> SendTemplate(EmailAddress from, EmailAddress to, string subject, string templateId, object templateData, string attachmentName = "", string attachmentBase64String = "")
        {
            var msg = new SendGridMessage();
            msg.SetFrom(from);
            msg.AddTo(to);
            msg.SetSubject(subject);
            msg.SetTemplateId(templateId);
            msg.SetTemplateData(templateData);
            if(attachmentName!=string.Empty && attachmentBase64String != string.Empty)
                msg.AddAttachment(attachmentName, attachmentBase64String);
            return await Send(msg);
        }
        public static async Task<Response> SendTransactional(EmailAddress from, EmailAddress to, string subject, string templateId, object templateData, string attachmentName = "", string attachmentBase64String = "")
        {
            var msg = new SendGridMessage();
            msg.SetFrom(from);
            msg.AddTo(to);
            msg.SetSubject(subject);
            msg.SetTemplateId(templateId);
            msg.SetTemplateData(templateData);
            if (attachmentName != string.Empty && attachmentBase64String != string.Empty)
                msg.AddAttachment(attachmentName, attachmentBase64String);
            return await Send(msg);
        }
        public static async Task<Response> SendLegacy(EmailAddress from, EmailAddress to, string subject, string templateId, List<Tuple<string,string>> substitutions, string attachmentName = "", string attachmentBase64String = "")
        {
            var msg = new SendGridMessage();
            msg.SetFrom(from);
            msg.AddTo(to);
            msg.SetSubject(subject);
            msg.SetTemplateId(templateId);
            if (attachmentName != string.Empty && attachmentBase64String != string.Empty)
                msg.AddAttachment(attachmentName, attachmentBase64String);
            substitutions.ForEach(sub => msg.AddSubstitution(sub.Item1, sub.Item2));
            return await Send(msg);
        }
    }
}
