namespace TP6_Gorojod_Schwartz_Waserman.Models;

public class Respuestas
{
    public int Id { get; set; }
    public string Respuesta { get; set; } = string.Empty;
    public bool EsCorrecta { get; set; }
    public int IdPregunta { get; set; }
}