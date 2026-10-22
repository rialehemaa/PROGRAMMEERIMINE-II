using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;

namespace Tooded_DB
{
    // Arve saatmine e-mailiga (Gmail SMTP)
    public static class Epost
    {
        // 1) Sisesta siia oma Gmail aadress
        public const string Saatja = "dariia.klimuk17@gmail.com";

        // 2) Sisesta siia Gmaili "rakenduse parool" (App password, 16 tähte), MITTE tavaline parool.
        //    Google konto -> Turvalisus -> 2-etapine kinnitus -> Rakenduste paroolid
        public const string RakenduseParool = "qnjz xsna hngz grpq";

        public static void Saada(string saaja, string pdfTee)
        {
            using var mail = new MailMessage();
            mail.From = new MailAddress(Saatja);
            mail.To.Add(saaja);
            mail.Subject = "Arve";
            mail.Body = "Arve on manuses.";
            mail.Attachments.Add(new Attachment(pdfTee));

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(Saatja, RakenduseParool.Replace(" ", "")),
                EnableSsl = true
            };
            smtp.Send(mail);
        }
    }
}