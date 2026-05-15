MANUAL DE DESPLIEGUE E INSTALACIÓN: A1CLICK

Versión: 1.0 (Release deploy) Arquitecto: Leonardo Ahumada Tecnología: SQL Server / .NET WinForms



Este documento detalla los pasos críticos para desplegar la solución en un entorno de producción limpio. Siga el orden estricto para garantizar la integridad del sistema.



1\. PREPARACIÓN DEL ENTORNO (WINDOWS \& SQL SERVER)

Antes de ejecutar cualquier script, se debe configurar el sistema operativo y el motor de base de datos para soportar las automatizaciones.



1.1 Estructura de Carpetas y Permisos

El sistema requiere una ruta específica para los respaldos automáticos.



Cree la carpeta en el disco raíz: C:\\A1Click\\backups



Permisos de Escritura (Crítico):



Haga clic derecho en la carpeta backups > Propiedades > Seguridad.



Clic en Editar > Agregar.



Escriba el usuario de servicio del Agente SQL (usualmente NT Service\\SQLSERVERAGENT o SYSTEM en instalaciones locales).



Otorgue permiso Control Total (Lectura/Escritura/Modificación).



Nota: Si omite este paso, el Job de Backup fallará con error "Access Denied".



1.2 Habilitación de SQL Server Agent

El motor de automatización debe estar corriendo como servicio de Windows.



Abra Servicios de Windows (Win + R > services.msc).



Busque SQL Server Agent (MSSQLSERVER).



Clic derecho > Propiedades.



Tipo de Inicio: Automático.



Estado: Clic en Iniciar.



2\. DESPLIEGUE DE BASE DE DATOS

Ejecute los scripts SQL en el orden numérico indicado abajo utilizando SQL Server Management Studio (SSMS).



FASE A: DDL (Estructura y Tablas)

Define el esquema, tablas y restricciones básicas.



01\_crear\_database.sql



Propósito: Crea la BD A1Click si no existe.



02\_confifuracion\_inicial.sql



Propósito: Configura Recovery Model Simple y Snapshot Isolation.



03\_tabla\_usuarios.sql



Propósito: Tabla de usuarios y roles de seguridad.



04\_tabla\_productos.sql



Propósito: Catálogo maestro de productos.



05\_tabla\_servicios\_catalogo.sql



Propósito: Catálogo de servicios intangibles.



tabla\_grupos.sql



Propósito: Definición de grupos de alerta (ej. CARPETAS).



06\_tabla\_ordenes.sql



Propósito: Cabeceras de venta.



07\_tabla\_items\_orden.sql



Propósito: Detalle de venta.



modificacio\_constraint\_precio0.sql



Propósito: Patch correctivo. Permite precio unitario 0 en ítems.



08\_tabla\_transacciones.sql



Propósito: Registro de pagos (Caja).



09\_tabla\_movimientos\_inventario.sql



Propósito: Kardex / Auditoría de stock.



10\_tabla\_parametros\_sistema.sql



Propósito: Tabla de configuración dinámica (Key-Value).



11\_tabla\_bandeja\_salida.sql



Propósito: Cola de correos (Outbox Pattern).



12\_tabla\_backup\_eventos.sql



Propósito: Log de auditoría de backups.



13\_alter\_table\_dbo.productos\_sku\_unique.sql



Propósito: Aplica restricción de unicidad en SKU.



FASE B: PROGRAMABILIDAD (Lógica de Negocio)

Instala tipos de datos, triggers y procedimientos almacenados.



14\_tt\_VentaLinea.sql



Propósito: Tipo TVP para transacciones atómicas.



27\_preparacion\_trigger\_carpetas\_config.sql



Propósito: Lógica inicial para agrupación de alertas.



28\_trg\_productos\_touch.sql



Propósito: Trigger de auditoría (Timestamp automático).



29\_trg\_stock\_critico\_productos.sql



Propósito: Trigger de alerta de stock individual.



30\_trg\_stock\_grupo\_carpetas.sql



Propósito: Trigger de alerta de stock grupal.



31\_trg\_backup\_eventos\_correo.sql



Propósito: Trigger de notificación de estado de backups.



22\_sp\_inventario\_movimiento\_aplicar.sql



Propósito: SP Core - Motor de inventario.



23\_sp\_venta\_registrar.sql



Propósito: SP Core - Orquestador de transacción de venta ACID.



24\_sp\_bandeja\_enviar\_dbmail.sql



Propósito: SP Infraestructura - Adaptador Database Mail.



25\_sp\_bandeja\_enviar\_pendientes.sql



Propósito: SP Infraestructura - Procesador de cola.



26\_sp\_reporte\_stock\_semanal\_enqueue.sql



Propósito: SP Reporting - Generador de reporte semanal.



FASE C: DATOS SEMILLA (Seeds)

Carga la configuración base y usuarios iniciales.



15\_param\_backup\_ruta.sql



Propósito: Configura la ruta C:\\A1Click\\backups.



16\_seed\_parametros\_sistema.sql



Propósito: Carga parámetros SMTP y reglas de negocio.



17\_seed\_productos\_desde\_excel...sql



Propósito: Carga masiva inicial de productos.



seed\_servicios.sql



Propósito: Carga inicial de catálogo de servicios.



UsuariosPorDefecto.sql



Propósito: Crea usuario admin (password hash por defecto).



creacion\_redmas\_productos\_criticos.sql



Propósito: Lógica de conversión de unidades (Resma/Insumo).



FASE D: AUTOMATIZACIÓN Y OPTIMIZACIÓN

Configura el Agente SQL y el rendimiento.



18\_config\_dbmail\_from\_parametros.sql



Propósito: Configura Database Mail usando los Params cargados en el paso 28.



19\_jobs\_a1click.sql



Propósito: Crea los Jobs (Alertas, Backups) en el Agente.



20\_job\_backup\_retencion.sql



Propósito: Job de limpieza de disco (política de retención).



21\_jobs\_attach\_local.sql



Propósito: Vincula los Jobs al servidor local actual.



32\_idx\_all.sql



Propósito: Crea índices de rendimiento para optimizar consultas.



3\. DESPLIEGUE DE APLICACIÓN (CLIENTE)

La aplicación A1Click no requiere instalación compleja, es portable.



Copiar Archivos:



Tome la carpeta Build o Release generada por Visual Studio.



Pegue la carpeta en la ruta de destino (Ej: C:\\Archivos de Programa\\A1Click\\).



Configuración de Conexión:



Localice el archivo A1Click.exe.config.



Edite la sección <connectionStrings> para apuntar a su servidor (ej. Data Source=.;Initial Catalog=A1Click;Integrated Security=True;).



Ejecución:



Ejecute A1Click.exe.



Credenciales por defecto: Usuario admin / Clave (la definida en el script de usuarios).



4\. VERIFICACIÓN POST-DESPLIEGUE

Para confirmar que el sistema está 100% operativo:



Prueba de Backup: Ejecute manualmente el Job JOB\_backup\_diario desde SSMS y verifique que aparezca el archivo .bak en la carpeta configurada.



Prueba de Acceso: Ingrese a la App con el usuario admin y visualice el listado de productos.



Monitoreo (Opcional): Use el script vista\_jobs.sql (en carpeta utilidades) para ver el estado de salud de los servicios de fondo.



Fin del Documento

