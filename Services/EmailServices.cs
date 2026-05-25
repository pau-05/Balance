using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Balance.API.Services
{
    public interface IEmailService
    {
        Task<bool> EnviarInvitacionAsync(string destinatario, string codigo, string centroNombre);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> EnviarInvitacionAsync(string destinatario, string codigo, string centroNombre)
        {
            try
            {
                // Validar destinatario
                if (string.IsNullOrEmpty(destinatario))
                {
                    _logger.LogError("El destinatario es nulo o vacío");
                    return false;
                }

                //Leer configuración con los nombres CORRECTOS
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPortStr = _configuration["EmailSettings:SmtpPort"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];      
                var senderName = _configuration["EmailSettings:SenderName"];        
                var password = _configuration["EmailSettings:Password"];
                var useSSLStr = _configuration["EmailSettings:UseSSL"];

                // Validar configuración
                if (string.IsNullOrEmpty(senderEmail))
                {
                    _logger.LogError("SenderEmail no está configurado");
                    return false;
                }

                if (string.IsNullOrEmpty(password))
                {
                    _logger.LogError("Password no está configurada");
                    return false;
                }

                int smtpPort = 587;
                if (!string.IsNullOrEmpty(smtpPortStr))
                    int.TryParse(smtpPortStr, out smtpPort);

                bool useSSL = true;
                if (!string.IsNullOrEmpty(useSSLStr))
                    bool.TryParse(useSSLStr, out useSSL);

                if (string.IsNullOrEmpty(senderName))
                    senderName = "Balance Terapéutico";

                _logger.LogInformation($"Configuración SMTP: Server={smtpServer}, Port={smtpPort}, Sender={senderEmail}, SSL={useSSL}");

                // Construir el email
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(senderName, senderEmail));  // ← Usar senderName y senderEmail
                email.To.Add(new MailboxAddress("", destinatario));
                email.Subject = $"✨ Invitación a Balance Terapéutico - {centroNombre}";

                // Cuerpo del email
                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <body>
                        <h2>✨ Balance Terapéutico</h2>
                        <p>Has sido invitado a unirte a <strong>{centroNombre}</strong>.</p>
                        <p>Tu código de verificación es:</p>
                        <h1 style='font-size: 48px;'>{codigo}</h1>
                        <p>Este código expirará en <strong>7 días</strong>.</p>
                    </body>
                    </html>";

                email.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

                // Enviar
                using var client = new SmtpClient();

                if (useSSL)
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                else
                    await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.Auto);

                await client.AuthenticateAsync(senderEmail, password);
                await client.SendAsync(email);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email enviado a {destinatario}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar email a {destinatario}: {ex.Message}");
                return false;
            }
        }
    }
}