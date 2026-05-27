using System.Text.Json;

namespace Balance.API.Converters
{
    public static class ConvertidorJsonAString
    {
        // Helper para convertir texto a JSON
        public static string ConvertirHorarioAJson(string textoHorario)
        {
            if (string.IsNullOrWhiteSpace(textoHorario))
                return "{}";

            var lineas = textoHorario.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var dict = new Dictionary<string, string>();

            foreach (var linea in lineas)
            {
                var partes = linea.Split(new[] { ':' }, 2);
                if (partes.Length == 2)
                    dict[partes[0].Trim().ToLower()] = partes[1].Trim();
            }

            return JsonSerializer.Serialize(dict);
        }

        // Helper para convertir JSON a texto (para mostrar en el frontend)
        public static string ConvertirJsonADocumentoTexto(JsonDocument? horarioJson)
        {
            if (horarioJson == null) return "";

            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(horarioJson);
                if (dict == null) return "";

                return string.Join("\n", dict.Select(kv => $"{kv.Key}: {kv.Value}"));
            }
            catch
            {
                return "";
            }
        }
    }
}
