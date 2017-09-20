using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace MemAnalyze.Services
{
    public class MailService
    {

        private string gmail_account = "andy5868968@gmail.com";
        private string gmail_Password = "wvagnmnakkejauly";
        private string gmail_mail = "andy5868968@gmail.com";
        //修改驗證信內容
        public string GetMailBody(string TempString, string UserAccount, string Validateurl)
        {
            TempString = TempString.Replace("{{UserAccount}}", UserAccount);
            TempString = TempString.Replace("{{Validateurl}}", Validateurl);
            //回傳結果
            return TempString;
        }
        //寄驗證信方法
        public void SendRegisterMail(string MailBody, string toMail)
        {
            SmtpClient smtpserver = new SmtpClient("smtp.gmail.com", 587);
            smtpserver.Credentials = new System.Net.NetworkCredential(gmail_account, gmail_Password);
            smtpserver.EnableSsl = true;
            MailMessage mail = new MailMessage(gmail_mail, toMail, "忘記密碼確認信", MailBody);
            mail.IsBodyHtml = true;
            smtpserver.Send(mail);
        }
    }
}