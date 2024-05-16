using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using simposio.Models;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using MessagingToolkit.QRCode.Codec;
using System;

namespace simposio.Services.Email
{
    public class EmailService
    {
        private readonly SMPTConfig _smptConfig; 
        private readonly IWebHostEnvironment _environment; 

        public EmailService(SMPTConfig smptConfig, IWebHostEnvironment environment)
        {
            _smptConfig = smptConfig;
            _environment = environment;
        }

        public async Task EnviarCorreo(Participante participante)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("SIMPOSIO UMG", _smptConfig.Email));
            email.To.Add(new MailboxAddress(participante.Nombres, participante.Correo));
            email.Subject = "Tu Registro Se recibio Correctamente";

            var htmlBody = $@"
                            <html>
                            <body>
                                <h1>HOLA {participante.Nombres} {participante.Apellidos}</h1>
                                <p>Gracias por Registrarte al Simposio 2024</p>
                                <p>Los Datos que recibimos son los siguientes:</p>
                                <ul>
                                    <li>Carnet: {participante.Carnet}</li>
                                    <li>Nombres: {participante.Nombres}</li>
                                    <li>Apellidos: {participante.Apellidos}</li>
                                    <li>Correo: {participante.Correo}</li>
                                </ul>
                                <img src='https://blobsimposio.blob.core.windows.net/imagenes/logo.png' alt='Logo' 
                                width='150'/>
                            </body>
                            </html>";

            email.Body = new TextPart("html") { Text = htmlBody };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(_smptConfig.Email, _smptConfig.Password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
        }

        public async Task EnviarCorreoDetallePago(DetallePago dp)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("SIMPOSIO UMG", _smptConfig.Email));
                email.To.Add(new MailboxAddress(dp.Participante.Nombres, dp.Participante.Correo));
                email.Subject = "Tu Registro Se recibio Correctamente";

                var htmlBody = new StringBuilder();
                htmlBody.Append($"<h1> Gracias {dp.Participante.Nombres} </h1>");
                htmlBody.Append("<h2> Tu detalle de pago ha sido recibido correctamente </h2>");
                htmlBody.Append("<br>");
                htmlBody.Append($"<h2> Total a Pagar Q{dp.Monto:n2}</h2>");
                htmlBody.Append("<br>");
                htmlBody.Append("<h3> Productos solicitados </h3>");
                htmlBody.Append("<table style='border-collapse: collapse; width: 100%;'>");
                htmlBody.Append("<tr style='border: 1px solid grey;'><th>Nombre</th><th>Imagen</th><th>Opción</th></tr>");

                foreach (var merch in dp.Merchandisings)
                {
                    htmlBody.Append("<tr style='border: 1px solid grey;'>");
                    htmlBody.Append($"<td style='border: 1px solid grey;'>{merch.merchandising.Nombre}</td>");
                    htmlBody.Append($"<td style='border: 1px solid grey;'><img src='{merch.merchandising.Imagen}' alt='Imagen' style='width:100px;'/></td>");
                    htmlBody.Append($"<td style='border: 1px solid grey;'>{(String.IsNullOrEmpty(merch.opcion) ? "" : merch.opcion)}</td>");
                    htmlBody.Append("</tr>");
                }

                htmlBody.Append("</table>");

                htmlBody.Append("<br> <br>");
                htmlBody.Append("<h2> Numeros de Cuenta para Depositar </h2>");

                email.Body = new TextPart("html") { Text = htmlBody.ToString() };

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    smtp.Authenticate(_smptConfig.Email, _smptConfig.Password);
                    await smtp.SendAsync(email);
                    smtp.Disconnect(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            
        }
    
        public async Task EnviarCorreoReciboPago(Participante participante)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("SIMPOSIO UMG", _smptConfig.Email));
            email.To.Add(new MailboxAddress(participante.Nombres, participante.Correo));
            email.Subject = "PAGO RECIBIDO - SIMPOSIO UMG";

            var htmlBody = $@"
                            <html>
                            <body>
                                <h1>HOLA {participante.Nombres} {participante.Apellidos}</h1>
                                <h2>Hemos recibido tu pago para el simposio 2024</h2>
                                <p>El pago recibido sera procesado y validado mas adelante.</p>
                                <p>Cuando termine el proceso y sea validado se te informara y recibiras informacion para ingresar al simposio</p>
                            </body>
                            </html>";

            email.Body = new TextPart("html") { Text = htmlBody };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(_smptConfig.Email, _smptConfig.Password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
        }

        public async Task EnviarCorreoPagoVerificado(Participante participante)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("SIMPOSIO UMG", _smptConfig.Email));
            email.To.Add(new MailboxAddress(participante.Nombres, participante.Correo));
            email.Subject = "PAGO VERIFICADO - SIMPOSIO UMG";

            var htmlBody = new StringBuilder();
            htmlBody.Append($@"
                            <html>
                            <body>
                                <h1>HOLA {participante.Nombres} {participante.Apellidos}</h1>
                                <h2>Tu pago ha sido verificado correctamente.</h2>
                                <h2>El siguiente codigo QR te servira para el ingreso del SIMPOSIO</h2>
                            </body>
                            </html>");
                                
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = htmlBody.ToString();

            GenerarQR(EncryptString(_smptConfig.CadenaEncriptacion, participante.Carnet), bodyBuilder);

            email.Body = bodyBuilder.ToMessageBody();

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(_smptConfig.Email, _smptConfig.Password);
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
        }

        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public void EnviarCorreoCertificado(Participante participante)
        {
            string filePath = Path.Combine(_environment.ContentRootPath, "Resources", "Images", "cm.pdf");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SIMPOSIO 2024", _smptConfig.Email));
            message.To.Add(new MailboxAddress("", participante.Correo));
            message.Subject = "Tu Certificado de Asistencia";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = "Aquí está tu certificado de asistencia al evento.";

            // Adjuntar el PDF desde el disco
            using (var fileStream = File.OpenRead(filePath))
            {
                bodyBuilder.Attachments.Add($"{participante.Carnet}_Certificado.pdf", fileStream, ContentType.Parse("application/pdf"));
                message.Body = bodyBuilder.ToMessageBody();

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    smtp.Authenticate(_smptConfig.Email, _smptConfig.Password);
                    smtp.Send(message);
                    smtp.Disconnect(true);
                }
            }
        }

        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static void GenerarQR(string texto, BodyBuilder bodyBuilder)
        {
            QRCodeEncoder encoder = new QRCodeEncoder();
            Bitmap img = encoder.Encode(texto);

            int padding = 20; // Puedes ajustar este valor según tus necesidades
            int newWidth = img.Width + 2 * padding;
            int newHeight = img.Height + 2 * padding;


            Bitmap paddedImg = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(paddedImg))
            {
                g.Clear(Color.White);

                g.DrawImage(img, padding, padding, img.Width, img.Height);
            }

            System.Drawing.Image Qr = (System.Drawing.Image)paddedImg;


            MimePart qrAttachment;
            MemoryStream qrStream = new MemoryStream();

            Qr.Save(qrStream, System.Drawing.Imaging.ImageFormat.Png);

            qrAttachment = new MimePart("image", "png")
            {
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = "codigo_qr.png"
            };

            qrAttachment.Content = new MimeContent(qrStream);

            bodyBuilder.Attachments.Add(qrAttachment);




        }
    }
}
