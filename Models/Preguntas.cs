namespace TP6_Gorojod_Schwartz_Waserman.Models;

public class Preguntas
{
    public int Id { get; set; }
    public string Pregunta { get; set; } = string.Empty;
    public List<Respuestas> Respuestas { get; set; } = new();
}