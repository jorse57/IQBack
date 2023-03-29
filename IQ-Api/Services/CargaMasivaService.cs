using IQ_Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace IQ_Api.Services
{
    public class CargaMasivaService : ICargaMasivaService
    {
        public async Task<List<respuestaCargas>> cargaArchivo()
        {
            List<respuestaCargas> resp = new List<respuestaCargas>();
            //string resp = "Respuesta servicio";
            string ubicacionArchivo = "C:\\Users\\USUARIO\\Desktop\\pruebaCarga.csv";
            System.IO.StreamReader archivo = new System.IO.StreamReader(ubicacionArchivo);
            string separador = ",";
            string linea;
            // Si el archivo no tiene encabezado, elimina la siguiente línea
            archivo.ReadLine(); // Leer la primera línea pero descartarla porque es el encabezado
            while ((linea = archivo.ReadLine()) != null)
            {
                string[] fila = linea.Split(separador);
                var j = new respuestaCargas()
                {
                    Descripcion = fila[0],
                    Precio = fila[1],
                    Existencia = fila[2]

                };
                resp.Add(j);
               // Console.WriteLine("Producto {0} con precio {1} y existencia {2}", descripcion, precio, existencia);
            }
            // Console.WriteLine("buen proceso");
            return resp;
        }
    }
}
