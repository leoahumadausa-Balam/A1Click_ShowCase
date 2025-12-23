using System;
using System.Windows.Forms;
using A1Click.WinForms.Views.Interfaces.Forms;
using A1Click.WinForms.UserControls;

namespace A1Click.WinForms
{
    /// <summary>
    /// Lógica de Interacción Global (UX) del Formulario Principal.
    /// Intercepta pulsaciones de teclado a bajo nivel para garantizar accesibilidad rápida.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Sobrescribe el procesamiento de comandos de Windows para capturar teclas 
        /// antes de que lleguen a los controles con foco (Textbox, botones, etc).
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // --- ATAJOS DE FUNCIÓN (Productividad Rápida) ---
            
            if (keyData == Keys.F1) 
            { 
                MostrarAyuda(); 
                return true; 
            }
            
            if (keyData == Keys.F2) 
            { 
                // Patrón Factory o Método Inteligente para instanciar ventas
                AbrirNuevaVentaInteligente(); 
                return true; 
            }
            
            if (keyData == Keys.F3) { AbrirCatalogoProductos(); return true; }
            if (keyData == Keys.F4) { AbrirCatalogoServicios(); return true; }
            if (keyData == Keys.F5) { BuscarVentasHistoricas(); return true; }
            
            // Diagnóstico y Seguridad
            if (keyData == Keys.F6) { EjecutarDiagnosticoConexion(); return true; }
            if (keyData == (Keys.Control | Keys.B)) { ForzarRespaldoManual(); return true; }

            // Gestión de Sesión
            if (keyData == Keys.F11 && _usuarioActual == null) 
            { 
                IniciarSesion(); 
                return true; 
            }
            if (keyData == Keys.F12) { CerrarSesion(); return true; }


            // --- LÓGICA DE NAVEGACIÓN (Tecla ESCAPE) ---
            // Permite "minimizar" una venta en curso sin perder los datos, 
            // transformándola de panel fijo a ventana flotante en segundo plano.
            if (keyData == Keys.Escape)
            {
                if (this.panelCentralVentas.Controls.Count > 0)
                {
                    Control controlActual = this.panelCentralVentas.Controls[0];

                    // CASO A: Es una Venta Activa -> NO CERRAR, SINO MINIMIZAR
                    if (controlActual is UCVenta ventaEnCurso)
                    {
                        // 1. La sacamos del panel principal
                        this.panelCentralVentas.Controls.Remove(ventaEnCurso);

                        // 2. La encapsulamos en un contenedor flotante (Patrón MDI simulado)
                        var formFlotante = new FormVentaModal(ventaEnCurso);
                        
                        string titulo = ventaEnCurso.Tag?.ToString() ?? $"Venta {DateTime.Now:HH:mm}";
                        formFlotante.Text = titulo;
                        formFlotante.Owner = this;

                        // 3. La registramos en la barra superior y ocultamos
                        RegistrarVentaEnBarraSuperior(formFlotante); 
                        formFlotante.Show();
                        formFlotante.Hide(); // Se va al "Dock" superior
                    }
                    // CASO B: Es cualquier otra pantalla (Catálogo, Reporte) -> CERRAR
                    else
                    {
                        controlActual.Dispose();
                        this.panelCentralVentas.Controls.Clear();
                    }

                    this.Focus();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}