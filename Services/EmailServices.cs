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
                // Validaciones iniciales
                if (string.IsNullOrEmpty(destinatario))
                {
                    _logger.LogError("El destinatario es nulo o vacío");
                    return false;
                }

                // Leer configuración con valores por defecto
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPortStr = _configuration["EmailSettings:SmtpPort"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var password = _configuration["EmailSettings:Password"];
                var useSSLStr = _configuration["EmailSettings:UseSSL"];

                //Validar que no falte configuración
                if (string.IsNullOrEmpty(smtpServer))
                {
                    _logger.LogError("EmailSettings:SmtpServer no está configurado");
                    return false;
                }

                if (string.IsNullOrEmpty(senderEmail))
                {
                    _logger.LogError("EmailSettings:SenderEmail no está configurado");
                    return false;
                }

                if (string.IsNullOrEmpty(password))
                {
                    _logger.LogError("EmailSettings:Password no está configurado");
                    return false;
                }

                int smtpPort = 587;
                if (!string.IsNullOrEmpty(smtpPortStr))
                {
                    int.TryParse(smtpPortStr, out smtpPort);
                }

                bool useSSL = true;
                if (!string.IsNullOrEmpty(useSSLStr))
                {
                    bool.TryParse(useSSLStr, out useSSL);
                }

                // Usar nombre por defecto si no está configurado
                if (string.IsNullOrEmpty(senderName))
                {
                    senderName = "Balance Terapéutico";
                }

                _logger.LogInformation($"Configuración SMTP: Server={smtpServer}, Port={smtpPort}, Sender={senderEmail}, SSL={useSSL}");

                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var subject = $"Invitación a Balance Terapéutico - {centroNombre}";
                var htmlBody = $@"
                    <h1>Bienvenido a Balance Terapéutico</h1>
                    <p>Has sido invitado a unirte al centro <strong>{centroNombre}</strong>.</p>
                    <p>Tu código de invitación es:{codigo}</p>
                    <h2 style='font-size: 32px; letter-spacing: 5px; background: #f0f0f0; padding: 10px; text-align: center;'>{codigo}</h2>
                    <p>Este código expira en 7 días.</p>
                    <p>Para completar tu registro, descarga la app Balance e ingresa este código.</p>
                    <hr/>
                    <p>Si no solicitaste esta invitación, ignora este mensaje.</p>
                ";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Balance Terapéutico", smtpSettings["FromEmail"]));
                message.To.Add(new MailboxAddress("", destinatario));
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    smtpSettings["Server"],
                    int.Parse(smtpSettings["Port"]),
                    SecureSocketOptions.StartTls
                );
                await client.AuthenticateAsync(smtpSettings["Username"], smtpSettings["Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email de invitación enviado a {destinatario}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar email a {destinatario}");
                return false;
            }
        }
    }
}