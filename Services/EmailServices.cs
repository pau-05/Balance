using System.Text;
using System.Text.Json;

namespace Balance.API.Services
{
    public interface IEmailService
    {
        Task<bool> EnviarInvitacionAsync(string destinatario, string codigo, string rol, string centroNombre);
    }

    public class EmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task<bool> EnviarInvitacionAsync(string destinatario, string codigo, string rol, string centroNombre)
        {
            try
            {
                var apiKey = _configuration["Resend:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Resend API Key no configurada");
                    return false;
                }

                var requestBody = new
                {
                    from = "Balance <onboarding@resend.dev>",
                    to = new[] { destinatario },
                    subject = $"Invitación a Balance Terapéutico - {centroNombre}",
                    html = $@"
                        <h2>Balance</h2>
                        <p>Has sido invitado a unirte a <strong>{centroNombre}</strong>.</p>
                        <p>Tu código de verificación es:</p>
                        <h1 style='font-size: 48px; letter-spacing: 5px;'>{codigo}</h1>
                        <p>Este código expirará en <strong>7 días</strong>.</p>"
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ Email enviado a {destinatario} via Resend");
                    return true;
                }
                else
                {
                    _logger.LogError($"Error Resend: {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar email a {destinatario}");
                return false;
            }
        }
    }
}