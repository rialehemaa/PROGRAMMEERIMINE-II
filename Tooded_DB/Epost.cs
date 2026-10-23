using System.Net;
using System.Net.Mail;

namespace Tooded_DB
{
    // Arve saatmine e-mailiga (Gmail SMTP).
    // Aadress ja rakenduse parool on failis AppData\Local\ToodedApp\epost.txt (mitte koodis ega GitHubis).
    // Fail: 1. rida = Gmaili aadress, 2. rida = rakenduse parool (App password)
    public static class Epost
    {
        private static string Fail => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ToodedApp", "epost.txt");

        public static void Saada(string saaja, string pdfTee)
        {
            if (!File.Exists(Fail))
                throw new Exception("Puudub fail " + Fail + "\nKirjuta sinna kaks rida: 1) Gmaili aadress 2) rakenduse parool.");

            var read = File.ReadAllLines(Fail);
            if (read.Length < 2)
                throw new Exception("Failis " + Fail + " peab olema kaks rida: aadress ja rakenduse parool.");

            string saatja = read[0].Trim();
            string parool = read[1].Replace(" ", "").Trim();

            using var mail = new MailMessage();
            mail.From = new MailAddress(saatja);
            mail.To.Add(saaja);
            mail.Subject = "Arve";
            mail.Body = "Arve on manuses.";
            mail.Attachments.Add(new Attachment(pdfTee));

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(saatja, parool),
                EnableSsl = true
            };
            smtp.Send(mail);
        }
    }
}