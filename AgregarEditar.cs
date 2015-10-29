using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibercionesCSharp
{
    public partial class AgregarEditar : Form
    {

        public Entidad entidad;
        public bool cancelar;
        
        public AgregarEditar()
        {
            InitializeComponent();            
        }

        private void AgregarEditar_Load(object sender, EventArgs e)
        {
            if (entidad != null)
            {
                txtNombre.Text = entidad.Nombre;
                txtScript.Text = entidad.Script;
                txtScript.ScrollBars = ScrollBars.Vertical;
                txtScript.KeyDown+=txtScript_KeyDown;
                txtNombre.KeyDown += txtScript_KeyDown;
            }
            
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            entidad.Nombre = txtNombre.Text;
            entidad.Script = txtScript.Text;
            cancelar = false;
         
            this.Close();
        }

        private void AgregarEditar_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        private void txtScript_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.A))
            {
                if (sender != null)
                    ((TextBox)sender).SelectAll();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }
}
