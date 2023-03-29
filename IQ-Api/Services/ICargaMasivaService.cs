using IQ_Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IQ_Api.Services
{
    public interface ICargaMasivaService
    {

        Task<List<respuestaCargas>> cargaArchivo();
    }
}
