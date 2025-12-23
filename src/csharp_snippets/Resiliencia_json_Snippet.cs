using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json; // Uso de librería nativa para alto rendimiento
using A1Click.Core.Entities; // Ajusta según tu namespace real
using A1Click.Core.Interfaces;

namespace A1Click.Core.Services
{
    /// <summary>
    /// Extracto del Servicio de Ventas enfocado en la Resiliencia (Offline-First).
    /// Permite suspender ventas y recuperarlas automáticamente ante fallos críticos.
    /// </summary>
    public class VentaService_Resiliencia
    {
        // ESTADO EN MEMORIA (Respaldado por JSON)
        private static List<Orden> _ventasSuspendidas = new List<Orden>();

        // RUTA DEL ARCHIVO (Dinámica: Se guarda junto al .exe del programa)
        private readonly string _rutaArchivoJson = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ventas_pendientes.json");

        public VentaService_Resiliencia()
        {
            // Al arrancar el servicio, intentamos recuperar lo guardado automáticamente
            CargarDesdeJson();
        }

        // ------------------------------------------------------------------
        // LÓGICA DE PERSISTENCIA LOCAL (RESILIENCIA)
        // ------------------------------------------------------------------

        /// <summary>
        /// Suspender una venta en curso guarda el estado en disco inmediatamente.
        /// Protege los datos ante cortes de energía o cierres inesperados.
        /// </summary>
        public void SuspenderVenta(Orden orden)
        {
            // Asignamos ID temporal si es nueva para rastrearla en RAM
            if (orden.IdOrden == 0)
            {
                // Usamos ticks de reloj para asegurar unicidad simple sin BD
                orden.IdOrden = (int)(DateTime.Now.Ticks % int.MaxValue);
            }

            _ventasSuspendidas.Add(orden);

            // ¡GUARDAMOS EN DISCO INMEDIATAMENTE!
            GuardarEnJson();
        }

        public List<Orden> ObtenerVentasSuspendidas()
        {
            return _ventasSuspendidas;
        }

        public void EliminarVentaSuspendida(int idTemporal)
        {
            var venta = _ventasSuspendidas.FirstOrDefault(v => v.IdOrden == idTemporal);
            if (venta != null)
            {
                _ventasSuspendidas.Remove(venta);
                // Actualizamos el respaldo físico
                GuardarEnJson();
            }
        }

        // --- MÉTODOS PRIVADOS DE MANEJO DE ARCHIVOS (I/O) ---

        private void GuardarEnJson()
        {
            try
            {
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(_ventasSuspendidas, opciones);
                
                // Sobrescribe el archivo de respaldo
                File.WriteAllText(_rutaArchivoJson, jsonString);
            }
            catch
            {
                // Estrategia Fail-Safe: Si falla el disco (permisos/espacio), 
                // el sistema sigue operando en RAM sin detenerse.
            }
        }

        private void CargarDesdeJson()
        {
            try
            {
                if (File.Exists(_rutaArchivoJson))
                {
                    string jsonString = File.ReadAllText(_rutaArchivoJson);
                    var listaRecuperada = JsonSerializer.Deserialize<List<Orden>>(jsonString);

                    if (listaRecuperada != null)
                    {
                        _ventasSuspendidas = listaRecuperada;
                    }
                }
            }
            catch
            {
                // Si el archivo está corrupto, iniciamos limpio para no bloquear la app
                _ventasSuspendidas = new List<Orden>();
            }
        }
    }
}