/* FRAGMENTO DE: sp_venta_registrar
   Lógica: Orquestación de transacción ACID con Table-Valued Parameters.
   Fuente: Proyecto A1Click - Leonardo Ahumada
*/

CREATE OR ALTER PROCEDURE dbo.sp_venta_registrar
    @CreadaPor        INT,
    @MetodoPago       VARCHAR(10),           
    @Monto            DECIMAL(12,2),         
    @Lineas           dbo.tt_VentaLinea READONLY, -- TVP recibido desde C#
    @IdOrden          INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Garantiza Rollback inmediato ante errores severos

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. VALIDACIÓN DE NEGOCIO (Ej: Método de Pago)
        IF @MetodoPago NOT IN ('EFECTIVO','TARJETA')
            THROW 50002, 'Método de pago inválido.', 1;

        -- 2. INSERTAR CABECERA
        INSERT INTO dbo.ordenes (total, creada_por, id_cliente, observaciones)
        VALUES (@Monto, @CreadaPor, @IdCliente, @Observaciones);
        
        SET @IdOrden = SCOPE_IDENTITY();

        -- 3. INSERTAR DETALLES (Bulk Insert desde TVP)
        INSERT INTO dbo.items_orden (id_orden, tipo_item, id_producto, id_servicio, cantidad, precio_unitario)
        SELECT @IdOrden, TipoItem, IdProducto, IdServicio, Cantidad, PrecioUnitario
        FROM @Lineas;

        -- 4. MOVIMIENTOS DE INVENTARIO (Auditoría y Descuento)
        -- Se utiliza una tabla temporal para capturar el estado previo/nuevo
        INSERT INTO dbo.movimientos_inventario
            (id_producto, tipo_movimiento, cantidad, stock_previo, stock_resultante, referencia_orden, creado_por)
        SELECT 
            IdProducto, 'VENTA', Cantidad, StockPrevio, StockResultante, @IdOrden, @CreadaPor
        FROM @MovimientosTemp; -- Calculado previamente en lógica interna

        -- 5. REGISTRO FINANCIERO (Caja)
        INSERT INTO dbo.transacciones (id_orden, metodo_pago, monto, pagado_por)
        VALUES (@IdOrden, @MetodoPago, @Monto, @CreadaPor);

        COMMIT TRANSACTION; -- Solo si todo es exitoso se guardan los cambios.
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW; -- Re-lanza el error a la aplicación C#
    END CATCH
END