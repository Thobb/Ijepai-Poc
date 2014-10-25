using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ijepai.Utilities;

namespace IjepaiMailer.Interface
{
    using System.Net.Mail;
    public interface IMailProvider
    {
        bool SendMail(MailMessage message, MailerPriorityFlag priority = MailerPriorityFlag.Normal);
    }
}
