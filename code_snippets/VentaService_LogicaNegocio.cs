using System;
using System.Threading.Tasks;
using A1Click.Core.Entities;
using A1Click.Core.Interfaces;

namespace A1Click.Core.Services
{
    /// <summary>
    /// Lógica de Negocio Core para el procesamiento de ventas.
    /// Asegura que se cumplan las reglas de negocio antes de persistir en BD.
    /// </summary>
    public class VentaService_LogicaNegocio
    {
        private readonly IVentaRepository _ventaRepo;

        public VentaService_LogicaNegocio(IVentaRepository ventaRepo)
        {
            _ventaRepo = ventaRepo;
        }

        /// <summary>
        /// Procesa una venta validando integridad de datos y montos.
        /// </summary>
        public async Task<int> ProcesarVentaAsync(Orden orden, Transaccion pago)
        {
            // 1. Validaciones de Reglas de Negocio (Domain Logic)
            if (orden.Items == null || orden.Items.Count == 0)
                throw new Exception("Regla de Negocio: No se puede procesar una venta sin productos.");

            if (orden.Total <= 0)
                throw new Exception("Regla de Negocio: El total de la venta debe ser positivo.");

            // Validación de cuadratura (evitar errores de redondeo o fraude)
            if (Math.Abs(orden.Total - pago.Monto) > 0.01m)
                throw new Exception("Error de Integridad: El monto del pago no coincide con el total de la orden.");

            // 2. Auditoría de tiempo (Usamos reloj del servidor/dominio, no del cliente local)
            orden.FechaCreacion = DateTime.Now; // O Reloj.Ahora si usas esa clase

            // 3. Persistencia Transaccional (Llamada al repositorio)
            // Esto dispara el guardado de Cabecera + Detalles + Movimiento de Caja en una sola transacción ACID.
            return await _ventaRepo.RegistrarVentaCompletaAsync(orden, pago);
        }
    }
}