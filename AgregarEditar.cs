using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
