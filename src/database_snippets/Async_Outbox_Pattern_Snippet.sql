/* FRAGMENTO DE: trg_stock_critico_productos
   Lógica: Detección de eventos y encolado asíncrono (Outbox Pattern).
   Fuente: Proyecto A1Click - Leonardo Ahumada
*/

CREATE OR ALTER TRIGGER dbo.trg_stock_critico_productos
ON dbo.productos
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Detectar productos que ACABAN de entrar en estado crítico
    -- (Stock actual <= Stock Mínimo)
    INSERT INTO dbo.bandeja_salida
        (para, asunto, cuerpo, estado, intentos, clave_dedup)
    SELECT
        @para_default,
        N'[A1Click] Stock crítico: ' + e.sku,
        N'Producto: ' + e.nombre + CHAR(13) + N'Stock actual: ' + CONVERT(NVARCHAR, e.stock_actual),
        'PENDIENTE', -- Estado inicial para el Job Processor
        0,
        -- CLAVE DE DEDUPLICACIÓN: 
        -- Evita spam masivo si el stock cambia muchas veces en segundos.
        N'CRITICO:' + e.sku 
    FROM inserted e
    WHERE e.stock_actual <= ISNULL(e.stock_minimo, 0)
    -- Validación Anti-Spam: No insertar si ya hay un correo pendiente para este SKU
    AND NOT EXISTS (
        SELECT 1 
        FROM dbo.bandeja_salida b
        WHERE b.clave_dedup = N'CRITICO:' + e.sku
          AND b.estado = 'PENDIENTE'
    );
END