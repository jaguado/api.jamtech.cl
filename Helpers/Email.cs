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
        private static readonly string _apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? throw new ApplicationException("SENDGRID_API_KEY variable not found");
        private static readonly SendGridClient _client = new SendGridClient(_apiKey);

        public static readonly EmailAddress defaultFrom = new EmailAddress("api@jamtech.cl", "JAMTech API");
        public static readonly EmailAddress defaultTo = new EmailAddress("jorge@jamtech.cl", "Jorge");
        public static void Test()
        {
            var subject = "Sending with SendGrid is Fun";
            var response = Send(defaultFrom, defaultTo, subject, "Test html content", "<b>Test html content</b>");
        }
        
        public static async Task<Response> Send(SendGridMessage msg)
        {
            return await _client.SendEmailAsync(msg);
        }
        public static async Task<Response> Send(EmailAddress from, EmailAddress to, string subject, string plainTextContent, string htmlContent, string attachmentName="", string attachmentBase64String="")
        {
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            if (attachmentName != string.Empty && attachmentBase64String != string.Empty)
                msg.AddAttachment(attachmentName, attachmentBase64String);
            return await Send(msg);
        }
        public static async Task<Response> SendTemplate(EmailAddress from, List<EmailAddress> to, string subject, string templateId, object templateData, string attachmentName = "", string attachmentBase64String = "")
        {
            var msg = new SendGridMessage();
            msg.SetFrom(from);
            msg.AddTos(to);
            msg.SetSubject(subject);
            msg.SetTemplateId(templateId);
            msg.SetTemplateData(templateData);
            if(attachmentName!=string.Empty && attachmentBase64String != string.Empty)
                msg.AddAttachment(attachmentName, attachmentBase64String);
            return await Send(msg);
        }
        public static async Task<Response> SendTransactional(EmailAddress from, List<EmailAddress> to, string subject, string templateId, object templateData, string attachmentName = "", string attachmentBase64String = "")
        {
            var msg = new SendGridMessage();
            msg.SetFrom(from);
            msg.AddTos(to);
            msg.SetSubject(subject);
            msg.SetTemplateId(templateId);
            msg.SetTemplateData(templateData);
            if (attachmentName != string.Empty && attachmentBase64String != string.Empty)
                msg.AddAttachment(attachmentName, attachmentBase64String);
            return await Send(msg);
        }
        public static async Task<Response> SendLegacy(EmailAddress from, List<EmailAddress> to, string subject, string templateId, IDictionary<string,string> substitutions, string attachmentName = "", string attachmentBase64String = "")
        {
            var msg = new SendGridMessage();
            msg.SetFrom(from);
            msg.AddTos(to);
            msg.SetSubject(subject);
            msg.SetTemplateId(templateId);
            if (attachmentName != string.Empty && attachmentBase64String != string.Empty)
                msg.AddAttachment(attachmentName, attachmentBase64String);
            substitutions.ToList().ForEach(sub => msg.AddSubstitution(sub.Key, sub.Value));
            return await Send(msg);
        }
    }
}
