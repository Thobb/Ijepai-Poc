using IjepaiMailer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace IjepaiMailer
{
    class SMTPMailProvider : IMailProvider
    {
        private string _smtp;
        private int _port;
        private string _username;
        private string _password;
        private bool _ssl;

        public SMTPMailProvider(string smtp, int port, string username, string password, bool ssl)
        {
            this._smtp = smtp;
            this._port = port;
            this._username = username;
            this._password = password;
            this._ssl = ssl;
        }

        public bool SendMail(MailMessage message, MailerPriorityFlag priority = MailerPriorityFlag.Normal)
        {
            bool isSuccess = false;
            try
            {
                SmtpClient sC = new SmtpClient(_smtp);
                sC.Port = Convert.ToInt32(_port);
                sC.Credentials = new NetworkCredential(_username, _password);
                sC.EnableSsl = _ssl;
                sC.Send(message);

                
                isSuccess = true;
            }
            catch (Exception e)
            {
                
            }
            return isSuccess;
        }
    }
}
