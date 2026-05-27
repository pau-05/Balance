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

        //Convertir JSON a texto (para mostrar en el frontend)
        public static string ConvertirJsonAString(JsonDocument? horarioJson)
        {
            if (horarioJson == null) return "";

            try
            {
                //Obtener el string JSON del JsonDocument
                var jsonString = horarioJson.RootElement.GetRawText();
                Console.WriteLine($"JSON recibido para convertir: {jsonString}");

                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                if (dict == null || dict.Count == 0) return "";

                Console.WriteLine($"Diccionario deserializado: {dict.Count} elementos");

                var resultado = string.Join("\n", dict.Select(kv => $"{kv.Key}: {kv.Value}"));
                Console.WriteLine($"Resultado: {resultado}");

                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ConvertirJsonAString: {ex.Message}");
                return "";
            }
        }
    }
}
