using Dapper;
using Microsoft.Data.SqlClient;

namespace TP6_Gorojod_Schwartz_Waserman.Models;

public class BD {
    private string connectionString = @"Server=localhost;Database=BaseSala;Trusted_Connection=True;TrustServerCertificate=True;"
;

    public int crearPartida(Partida partida)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = @"INSERT INTO Partida (FechaInicio, HoraInicio, IdSala, EstadoActual, NombreJugador, Vidas)
                                          VALUES (@fechaInicio, @horaInicio, @IdSala, @estadoActual, @nombreJugador, @Vidas);
                            SELECT CAST(SCOPE_IDENTITY() as int);";
            return connection.ExecuteScalar<int>(query, partida);
        }
    }

    public int PerderVida(int partidaId)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            const string query = @"UPDATE Partida
                                   SET Vidas = CASE WHEN Vidas > 0 THEN Vidas - 1 ELSE 0 END
                                   WHERE Id = @partidaId;
                                   SELECT Vidas FROM Partida WHERE Id = @partidaId;";
            return connection.ExecuteScalar<int>(query, new { partidaId });
        }
    }

    public void perderPartida(int Vidas) {
        using (SqlConnection connection = new SqlConnection(connectionString)) {
            string query = @"DELETE * FROM Partida WHERE Vidas = 0;";
        }
    }
    
    public bool TieneAcceso(int partidaId, int IdSala)
    {
        if (partidaId == null)
            return false;
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT COUNT(*) FROM Partida WHERE Id = @partidaId AND IdSala = @pIdSala";
            int count = connection.ExecuteScalar<int>(query, new { partidaId, pIdSala = IdSala });
            return count > 0;
        }
    }

    public void pasarSala(int partidaId, int nuevaSala)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "UPDATE Partida SET IdSala = @nuevaSala WHERE Id = @partidaId";
            connection.Execute(query, new { partidaId, nuevaSala });
        }
    }

    public Partida ObtenerPartidaPorNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return null;
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT TOP 1 * FROM Partida WHERE NombreJugador = @nombre ORDER BY Id DESC";
            return connection.QueryFirstOrDefault<Partida>(query, new { nombre });
        }
    }

    public Partida ObtenerPartidaPorId(int id)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            return connection.QueryFirstOrDefault<Partida>("SELECT * FROM Partida WHERE Id = @id", new { id });
        }
    }

    public List<Preguntas> ObtenerPreguntasSala1()
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            const string query = @"SELECT Id, Pregunta
                                   FROM Preguntas
                                   ORDER BY Id";
            var preguntas = connection.Query<Preguntas>(query).ToList();

            foreach (var pregunta in preguntas)
            {
                pregunta.Respuestas = ObtenerRespuestasPorPregunta(pregunta.Id);
            }

            return preguntas;
        }
    }

    public List<Respuestas> ObtenerRespuestasPorPregunta(int idPregunta)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            const string query = @"SELECT Id,
                                          Respuesta,
                                          EsCorrecta,
                                          IdPregunta
                                   FROM Respuestas
                                   WHERE IdPregunta = @idPregunta
                                   ORDER BY Id";
            return connection.Query<Respuestas>(query, new { idPregunta }).ToList();
        }
    }

    public Respuestas ObtenerRespuestaPorId(int idRespuesta)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            const string query = @"SELECT Id,
                                          Respuesta,
                                          EsCorrecta,
                                          IdPregunta
                                   FROM Respuestas
                                   WHERE Id = @idRespuesta";
            return connection.QueryFirstOrDefault<Respuestas>(query, new { idRespuesta });
        }
    }

    public SalaTriviaViewModel ObtenerTriviaSala1()
    {
        return new SalaTriviaViewModel
        {
            Preguntas = ObtenerPreguntasSala1()
        };
    }
}