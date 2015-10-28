using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Deployment;
using System.Deployment.Application;

namespace LibercionesCSharp
{
    public partial class Form1 : Form
    {
        public string conexionBase = "Data Source=TestDBServer;Initial Catalog=Liberacion;User Id=Dev;Password=Chetos@@2015;";

        public int contador = 1;
        public int liberacionID = 0;
        public int ultAct = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(conexionBase);
            DataSet BaseDeDatos = new DataSet();
            DataTable dt = new DataTable();

            try
            {

                dt.Columns.Add("Delimitador");
                dt.Columns.Add("DelimitadorID");

                DataRow dr = dt.NewRow();

                dr["Delimitador"] = "Seleccionar";
                dr["DelimitadorID"] = "";
                dt.Rows.Add(dr);

                dr = dt.NewRow();
                dr["Delimitador"] = "Vacio";
                dr["DelimitadorID"] = "Vacio";
                dt.Rows.Add(dr);

                dr = dt.NewRow();
                dr["Delimitador"] = "GO";
                dr["DelimitadorID"] = "GO";
                dt.Rows.Add(dr);

                //dr = dt.NewRow();
                //dr["Delimitador"] = "--GO--";
                //dr["DelimitadorID"] = "--GO--";
                //dt.Rows.Add(dr);

                this.comboBox2.DataSource = dt;
                this.comboBox2.DisplayMember = "Delimitador";
                this.comboBox2.ValueMember = "DelimitadorID";
                               
                string query = "select * from BaseDeDatos where estatus = 1";

                SqlCommand sqlc = new SqlCommand(query, conn);
                sqlc.CommandType = CommandType.Text;
                conn.Open();
                sqlc.ExecuteNonQuery();
                SqlDataAdapter adapter = new SqlDataAdapter(sqlc);

                adapter.Fill(BaseDeDatos, "BaseDeDatos");
                conn.Close();

                comboBox1.DataSource = BaseDeDatos.Tables[0];
                comboBox1.DisplayMember = "Nombre";
                comboBox1.ValueMember = "Conexion";

                this.button2.Enabled = false;
                this.button3.Enabled = false;
                this.button4.Enabled = false;

                this.textBox1.Focus();
                this.progressBar1.Visible = false;

            }
            catch (Exception ex)
            {
                throw ex;
            }    
        }

        //BUSCAR FOLIO NUEVO O EXISTENTE
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty){
                Buscar();
                //this.button2.Enabled = false;
                //this.button3.Enabled = false;
                //this.button4.Enabled = false;

                //this.textBox1.Focus();
                //this.progressBar1.Visible = false;
            }
            else
                MessageBox.Show("Indicar el numero de folio.");
        }

        //LIBERAR FOLIO EN BD
        private void button4_Click(object sender, EventArgs e)
        {
            if (this.comboBox2.SelectedValue.ToString() == string.Empty)
            {
                MessageBox.Show("Seleccione un delimitador");
                return;
            }

            DialogResult result = MessageBox.Show("Seguro que desea liberar?", "Liberar", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result.Equals(DialogResult.OK))
            {


                using (SqlConnection conn = new SqlConnection(comboBox1.SelectedValue.ToString()))
                {
                    conn.Open();
                    SqlCommand sqlc = conn.CreateCommand();

                    SqlTransaction transaction;

                    transaction = conn.BeginTransaction("transaccion");

                    sqlc.Connection = conn;
                    sqlc.Transaction = transaction;
                    int detalleid = 0;

                    try
                    {
                        progressBar1.Visible = true;
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = dataGridView1.Rows.Count - 1;
                        progressBar1.Value = 0;
                        progressBar1.Step = 1;

                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {

                            progressBar1.PerformStep();
                            if (dataGridView1.Rows[i].Cells["Liberar"].Value != null)
                            {
                                detalleid = int.Parse(dataGridView1.Rows[i].Cells["LiberacionDetalleID"].Value.ToString());
                                //sqlc.CommandText = dataGridView1.Rows[i].Cells["Script"].Value.ToString();
                                string[] commands = null;
                                string delimitador = comboBox2.SelectedValue.ToString();
                                if (delimitador == "GO")
                                {
                                    commands = dataGridView1.Rows[i].Cells["Script"].Value.ToString().Split(new string[] { "GO\r\n", "GO ", "GO\t" }, StringSplitOptions.RemoveEmptyEntries);
                                }
                                else if (delimitador == "--GO--")
                                {
                                    commands = dataGridView1.Rows[i].Cells["Script"].Value.ToString().Split(new string[] { delimitador }, StringSplitOptions.RemoveEmptyEntries);
                                }
                                else
                                {
                                    commands = new string[1];
                                    commands[0] = dataGridView1.Rows[i].Cells["Script"].Value.ToString();
                                }
                                //string[] commands = dataGridView1.Rows[i].Cells["Script"].Value.ToString().Split(new string[] { "--GO--" }, StringSplitOptions.RemoveEmptyEntries);
                                //string[] commands = dataGridView1.Rows[i].Cells["Script"].Value.ToString().Split(new string[] { comboBox2.SelectedValue.ToString() }, StringSplitOptions.RemoveEmptyEntries);

                                foreach (string c in commands)
                                {
                                    sqlc.CommandText = c;
                                    sqlc.ExecuteNonQuery();
                                }

                                //sqlc.CommandText = "update LiberacionDetalle set Error = '',FechaLiberacion = getdate(),Estatus = 2 where LiberacionDetalleID = " + detalleid + " and LiberacionID = " + liberacionID;
                                //sqlc.ExecuteNonQuery();
                                UpdateLiberador("update LiberacionDetalle set Error = '',FechaLiberacion = getdate(),Estatus = 2 where LiberacionDetalleID = " + detalleid + " and LiberacionID = " + liberacionID);
                            }
                        }

                        //sqlc.CommandText = "update Liberacion set Estatus = 2, FechaLiberacion = getdate() where LiberacionID = " + liberacionID;
                        //sqlc.ExecuteNonQuery(); 
                        UpdateLiberador("update Liberacion set Estatus = 2, FechaLiberacion = getdate() where LiberacionID = " + liberacionID);

                        transaction.Commit();
                        conn.Close();

                        MessageBox.Show("El folio se ejecuto correctamente");
                        Buscar();
                        this.button4.Enabled = false;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        //sqlc.CommandText = "update LiberacionDetalle set Error = '" + ex.ToString().Replace("'", " ").ToString() +"',Estatus = 3 where LiberacionDetalleID = " + detalleid + " and LiberacionID = " + liberacionID;
                        //sqlc.ExecuteNonQuery();
                        UpdateLiberador("update LiberacionDetalle set Error = '" + ex.ToString().Replace("'", " ").ToString() + "',Estatus = 3 where LiberacionDetalleID = " + detalleid + " and LiberacionID = " + liberacionID);

                        conn.Close();

                        MessageBox.Show("Hubo errores en los detalles");
                        Buscar();
                    }

                }
            }
        }

        public void UpdateLiberador(string query)
        {
            SqlConnection conn = new SqlConnection(conexionBase);
               
            SqlCommand sqlc = new SqlCommand(query, conn);
            sqlc.CommandType = CommandType.Text;
            conn.Open();
            sqlc.ExecuteNonQuery();
            conn.Close();
        }

        //LIMPIAR CONTROLES
        private void button5_Click(object sender, EventArgs e)
        {
            limpiar();            
        }

        //GUARDAR FOLIO CON DETALLE
        private void button3_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox.Show("Seguro que desea guardar?", "Save", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result.Equals(DialogResult.OK))
            {
                //Do something
                if (this.liberacionID == 0)
                {
                    //NUEVO
                    DataSet Liberacion = new DataSet();
                    using (SqlConnection conn = new SqlConnection(conexionBase))
                    {
                        string query = "Liberacion_Insert";

                        SqlCommand sqlc = new SqlCommand(query, conn);
                        sqlc.CommandType = CommandType.StoredProcedure;

                        conn.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter(sqlc);

                        adapter.SelectCommand.Parameters.Add("@Folio", SqlDbType.Int).Value = this.textBox1.Text;
                        adapter.SelectCommand.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = this.textBox2.Text;
                        adapter.SelectCommand.Parameters.Add("@BD", SqlDbType.Int).Value = 1;

                        adapter.Fill(Liberacion, "Liberacion");

                        liberacionID = int.Parse(Liberacion.Tables[0].Rows[0]["ID"].ToString());

                        query = "LiberacionDetalle_Insert";

                        progressBar1.Visible = true;
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = dataGridView1.Rows.Count - 1;
                        progressBar1.Value = 0;
                        progressBar1.Step = 1;

                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            progressBar1.PerformStep();
                            if ((dataGridView1.Rows[i].Cells["Eliminar"].Value == null))
                            {
                                sqlc = new SqlCommand(query, conn);
                                sqlc.CommandType = CommandType.StoredProcedure;
                                adapter = new SqlDataAdapter(sqlc);
                                adapter.SelectCommand.Parameters.Add("@LiberacionDetalleID", SqlDbType.Int).Value = int.Parse(dataGridView1.Rows[i].Cells["LiberacionDetalleID"].Value.ToString());
                                adapter.SelectCommand.Parameters.Add("@LiberacionID", SqlDbType.Int).Value = liberacionID;
                                adapter.SelectCommand.Parameters.Add("@Nombre", SqlDbType.NVarChar, 200).Value = dataGridView1.Rows[i].Cells["Nombre"].Value.ToString();
                                adapter.SelectCommand.Parameters.Add("@Script", SqlDbType.NVarChar, int.MaxValue).Value = dataGridView1.Rows[i].Cells["Script"].Value.ToString();

                                sqlc.ExecuteNonQuery();
                            }
                        }

                        conn.Close();

                        MessageBox.Show("El folio se guardo correctamente");
                        Buscar();
                    }

                }
                else
                {
                    //EDITADO
                    using (SqlConnection conn = new SqlConnection(conexionBase))
                    {
                        SqlCommand sqlc;
                        SqlDataAdapter adapter;
                        DataSet Liberacion = new DataSet();

                        conn.Open();

                        sqlc = new SqlCommand("select cast(L.UltActualizacion as int) UltActualizacion from Liberacion L inner join LiberacionDetalle LD on LD.liberacionID = L.LiberacionID where L.Folio =" + this.textBox1.Text + "group by L.LiberacionID,L.Folio,L.Estatus,LD.LiberacionDetalleID,LD.Nombre, LD.Script, LD.Estatus , LD.Error,L.UltActualizacion ", conn);
                        sqlc.ExecuteNonQuery();
                        adapter = new SqlDataAdapter(sqlc);
                        adapter.Fill(Liberacion, "Liberacion");

                        var LiberacionUltimaAct = (from d in Liberacion.Tables[0].AsEnumerable()
                                                   select new
                                                   {
                                                       UltActualizacion = int.Parse(d.Field<int>("UltActualizacion").ToString()),
                                                   }).ToList();

                        if (ultAct != LiberacionUltimaAct[0].UltActualizacion)
                        {
                            conn.Close();
                            MessageBox.Show("El folio fue editado por otra persona");
                            Buscar();
                        }
                        else
                        {
                            string query = string.Empty;

                            progressBar1.Visible = true;
                            progressBar1.Minimum = 0;
                            progressBar1.Maximum = dataGridView1.Rows.Count - 1;
                            progressBar1.Value = 0;
                            progressBar1.Step = 1;

                            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                            {
                                progressBar1.PerformStep();

                                if (bool.Parse(dataGridView1.Rows[i].Cells["Nuevo"].Value.ToString()) == true)
                                    query = "LiberacionDetalle_Insert";
                                else
                                    query = "LiberacionDetalle_Update";

                                if (dataGridView1.Rows[i].Cells["Eliminar"].Value == null && bool.Parse(dataGridView1.Rows[i].Cells["Modificado"].Value.ToString()) == true)
                                {
                                    //Editar
                                    sqlc = new SqlCommand(query, conn);
                                    sqlc.CommandType = CommandType.StoredProcedure;
                                    adapter = new SqlDataAdapter(sqlc);
                                    adapter.SelectCommand.Parameters.Add("@LiberacionID", SqlDbType.Int).Value = liberacionID;
                                    adapter.SelectCommand.Parameters.Add("@LiberacionDetalleID", SqlDbType.Int).Value = int.Parse(dataGridView1.Rows[i].Cells["LiberacionDetalleID"].Value.ToString());
                                    adapter.SelectCommand.Parameters.Add("@Nombre", SqlDbType.NVarChar, 200).Value = dataGridView1.Rows[i].Cells["Nombre"].Value.ToString();
                                    adapter.SelectCommand.Parameters.Add("@Script", SqlDbType.NVarChar, int.MaxValue).Value = dataGridView1.Rows[i].Cells["Script"].Value.ToString();

                                    sqlc.ExecuteNonQuery();
                                }
                                //else if (dataGridView1.Rows[i].Cells["Eliminar"].Value == null && bool.Parse(dataGridView1.Rows[i].Cells["Nuevo"].Value.ToString()) == false && bool.Parse(dataGridView1.Rows[i].Cells["Nuevo"].Value.ToString()) == true)
                                //{

                                //    //Insertar
                                //    sqlc = new SqlCommand(query, conn);
                                //    sqlc.CommandType = CommandType.StoredProcedure;
                                //    adapter = new SqlDataAdapter(sqlc);
                                //    adapter.SelectCommand.Parameters.Add("@LiberacionID", SqlDbType.Int).Value = liberacionID;
                                //    adapter.SelectCommand.Parameters.Add("@LiberacionDetalleID", SqlDbType.Int).Value = int.Parse(dataGridView1.Rows[i].Cells["LiberacionDetalleID"].Value.ToString());
                                //    adapter.SelectCommand.Parameters.Add("@Nombre", SqlDbType.NVarChar, 200).Value = dataGridView1.Rows[i].Cells["Nombre"].Value.ToString();
                                //    adapter.SelectCommand.Parameters.Add("@Script", SqlDbType.NVarChar, int.MaxValue).Value = dataGridView1.Rows[i].Cells["Script"].Value.ToString();

                                //    sqlc.ExecuteNonQuery();
                                //}
                                //else if ((dataGridView1.Rows[i].Cells["Eliminar"].Value.ToString() == "True" && bool.Parse(dataGridView1.Rows[i].Cells["Nuevo"].Value.ToString()) == true))
                                //{
                                //    //Nada
                                //}
                                else if ((dataGridView1.Rows[i].Cells["Eliminar"].Value == null))
                                {

                                }
                                else if ((dataGridView1.Rows[i].Cells["Eliminar"].Value.ToString() == "True" && bool.Parse(dataGridView1.Rows[i].Cells["Nuevo"].Value.ToString()) == false))
                                {
                                    //Eliminar de BD
                                    sqlc = new SqlCommand("delete from LiberacionDetalle where LiberacionID = " + liberacionID + " AND LiberacionDetalleID = " + int.Parse(dataGridView1.Rows[i].Cells["LiberacionDetalleID"].Value.ToString()), conn);
                                    sqlc.ExecuteNonQuery();

                                }

                            }

                            sqlc = new SqlCommand("update Liberacion set Estatus = 1,Descripcion ='" + this.textBox2.Text + "' where LiberacionID = " + liberacionID, conn);
                            sqlc.ExecuteNonQuery();

                            conn.Close();

                            MessageBox.Show("El folio se edito correctamente");
                            Buscar();

                        }
                    }

                }

                this.button4.Enabled = true;
            }
        }

        //AGREGAR DETALLE DE FOLIO(QUERY, SP, FUNCION ETC)
        private void button2_Click(object sender, EventArgs e)
        {
            Entidad entidadPadre = new Entidad();
            entidadPadre.LiberacionID = 0;
            entidadPadre.Folio = int.Parse(textBox1.Text);
            entidadPadre.LiberacionEstatus = 1;
            entidadPadre.LiberacionEstatusString = estatus.Activo.ToString();
            entidadPadre.LiberacionDetalleID = contador;
            entidadPadre.Nombre = string.Empty;
            entidadPadre.Script = string.Empty;
            entidadPadre.DetalleEstatus = (int)estatus.Activo;
            entidadPadre.DetalleEstatusString = estatus.Activo.ToString();
            entidadPadre.Error = string.Empty;
            entidadPadre.Nuevo = true;
            entidadPadre.Modificado = true;
            entidadPadre.Descripcion = string.Empty;

            AgregarEditar AEForm = new AgregarEditar();
            AEForm.entidad = entidadPadre;
            AEForm.cancelar = true;
            AEForm.ShowDialog();

            if (AEForm.cancelar == true)
                return;
            
            List<Entidad> list = new List<Entidad>();

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                entidadPadre = new Entidad();

                entidadPadre.LiberacionID = int.Parse(dataGridView1.Rows[i].Cells["LiberacionID"].Value.ToString());
                entidadPadre.Folio = int.Parse(dataGridView1.Rows[i].Cells["Folio"].Value.ToString());
                entidadPadre.LiberacionEstatus = int.Parse(dataGridView1.Rows[i].Cells["LiberacionEstatus"].Value.ToString());
                entidadPadre.LiberacionEstatusString = dataGridView1.Rows[i].Cells["LiberacionEstatusString"].Value.ToString();
                entidadPadre.LiberacionDetalleID = int.Parse(dataGridView1.Rows[i].Cells["LiberacionDetalleID"].Value.ToString());
                entidadPadre.Nombre = dataGridView1.Rows[i].Cells["Nombre"].Value.ToString();
                entidadPadre.Script = dataGridView1.Rows[i].Cells["Script"].Value.ToString();
                entidadPadre.DetalleEstatus = int.Parse(dataGridView1.Rows[i].Cells["DetalleEstatus"].Value.ToString());
                entidadPadre.DetalleEstatusString = dataGridView1.Rows[i].Cells["DetalleEstatusString"].Value.ToString();
                entidadPadre.Error = dataGridView1.Rows[i].Cells["Error"].Value.ToString();
                entidadPadre.Nuevo = bool.Parse(dataGridView1.Rows[i].Cells["Nuevo"].Value.ToString());
                entidadPadre.Modificado = bool.Parse(dataGridView1.Rows[i].Cells["Modificado"].Value.ToString());
                entidadPadre.Descripcion = dataGridView1.Rows[i].Cells["Descripcion"].Value.ToString();

                list.Add(entidadPadre);

            }

            entidadPadre = new Entidad();

            entidadPadre.LiberacionID = AEForm.entidad.LiberacionID;
            entidadPadre.Folio = AEForm.entidad.Folio;
            entidadPadre.LiberacionEstatus = AEForm.entidad.LiberacionEstatus;
            entidadPadre.LiberacionEstatusString = AEForm.entidad.LiberacionEstatusString;
            entidadPadre.LiberacionDetalleID = AEForm.entidad.LiberacionDetalleID;
            entidadPadre.Nombre = AEForm.entidad.Nombre;
            entidadPadre.Script = AEForm.entidad.Script;
            entidadPadre.DetalleEstatus = AEForm.entidad.DetalleEstatus;
            entidadPadre.DetalleEstatusString = AEForm.entidad.DetalleEstatusString;
            entidadPadre.Error = AEForm.entidad.Error;
            entidadPadre.Nuevo = AEForm.entidad.Nuevo;
            entidadPadre.Modificado = AEForm.entidad.Modificado;
            entidadPadre.Descripcion = AEForm.entidad.Descripcion;

            list.Add(entidadPadre);

            dataGridView1.Columns.Clear();
            dataGridView1.DataSource = list;

            var removeColumn = new DataGridViewCheckBoxColumn
            {
                Name = "Eliminar",
                DataPropertyName = "Eliminar"
            };

            var LiberarColumn = new DataGridViewCheckBoxColumn
            {
                Name = "Liberar",
                DataPropertyName = "Liberar"
            };

            var editColumn = new DataGridViewButtonColumn
            {
                Text = "Editar",
                UseColumnTextForButtonValue = true,
                Name = "Editar",
                DataPropertyName = "Editar"
            };

            dataGridView1.Columns.Add(editColumn);
            dataGridView1.Columns.Add(removeColumn);
            dataGridView1.Columns.Add(LiberarColumn);
            dataGridView1.Columns["LiberacionID"].Visible = false;
            dataGridView1.Columns["Folio"].Visible = false;
            dataGridView1.Columns["LiberacionEstatus"].Visible = false;
            dataGridView1.Columns["LiberacionEstatusString"].Visible = false;
            dataGridView1.Columns["DetalleEstatus"].Visible = false;
            dataGridView1.Columns["LiberacionDetalleID"].Visible = false;
            dataGridView1.Columns["DetalleEstatusString"].Visible = false;
            dataGridView1.Columns["Nuevo"].Visible = false;
            dataGridView1.Columns["Modificado"].Visible = false;
            dataGridView1.Columns["Descripcion"].Visible = false;

            this.button3.Enabled = true;
            contador = contador + 1;
        }
        //EDITAR UN DETALLE
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //int col = this.dataGridView1.CurrentCell.ColumnIndex;
            //int row = this.dataGridView1.CurrentCell.RowIndex;

            //Entidad entidadPadre = new Entidad();
            //entidadPadre.LiberacionID = int.Parse(dataGridView1.Rows[row].Cells["LiberacionID"].Value.ToString());
            //entidadPadre.Folio = int.Parse(dataGridView1.Rows[row].Cells["Folio"].Value.ToString());
            //entidadPadre.LiberacionEstatus = int.Parse(dataGridView1.Rows[row].Cells["LiberacionEstatus"].Value.ToString());
            //entidadPadre.LiberacionEstatusString = dataGridView1.Rows[row].Cells["LiberacionEstatusString"].Value.ToString();
            //entidadPadre.LiberacionDetalleID = int.Parse(dataGridView1.Rows[row].Cells["LiberacionDetalleID"].Value.ToString());
            //entidadPadre.Nombre = dataGridView1.Rows[row].Cells["Nombre"].Value.ToString();
            //entidadPadre.Script = dataGridView1.Rows[row].Cells["Script"].Value.ToString();
            //entidadPadre.DetalleEstatus = int.Parse(dataGridView1.Rows[row].Cells["DetalleEstatus"].Value.ToString());
            //entidadPadre.DetalleEstatusString = dataGridView1.Rows[row].Cells["DetalleEstatusString"].Value.ToString();
            //entidadPadre.Error = dataGridView1.Rows[row].Cells["Error"].Value.ToString();
            //entidadPadre.Nuevo = bool.Parse(dataGridView1.Rows[row].Cells["Nuevo"].Value.ToString());

            //AgregarEditar AEForm = new AgregarEditar();
            //AEForm.entidad = entidadPadre;
            //AEForm.ShowDialog();

            //List<Entidad> list = new List<Entidad>();

            //for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //{
            //    entidadPadre = new Entidad();

            //    if (AEForm.entidad.LiberacionDetalleID.ToString() == dataGridView1.Rows[i].Cells["LiberacionDetalleID"].Value.ToString())
            //    {
            //        entidadPadre.LiberacionID = AEForm.entidad.LiberacionID;
            //        entidadPadre.Folio = AEForm.entidad.Folio;
            //        entidadPadre.LiberacionEstatus = AEForm.entidad.LiberacionEstatus;
            //        entidadPadre.LiberacionEstatusString = AEForm.entidad.LiberacionEstatusString;
            //        entidadPadre.LiberacionDetalleID = AEForm.entidad.LiberacionDetalleID;
            //        entidadPadre.Nombre = AEForm.entidad.Nombre;
            //        entidadPadre.Script = AEForm.entidad.Script;
            //        entidadPadre.DetalleEstatus = AEForm.entidad.DetalleEstatus;
            //        entidadPadre.DetalleEstatusString = AEForm.entidad.DetalleEstatusString;
            //        entidadPadre.Error = AEForm.entidad.Error;
            //        entidadPadre.Nuevo = AEForm.entidad.Nuevo;    
            //    }
            //    else
            //    {
            //        entidadPadre.LiberacionID = int.Parse(dataGridView1.Rows[i].Cells["LiberacionID"].Value.ToString());
            //        entidadPadre.Folio = int.Parse(dataGridView1.Rows[i].Cells["Folio"].Value.ToString());
            //        entidadPadre.LiberacionEstatus = int.Parse(dataGridView1.Rows[i].Cells["LiberacionEstatus"].Value.ToString());
            //        entidadPadre.LiberacionEstatusString = dataGridView1.Rows[i].Cells["LiberacionEstatusString"].Value.ToString();
            //        entidadPadre.LiberacionDetalleID = int.Parse(dataGridView1.Rows[i].Cells["LiberacionDetalleID"].Value.ToString());
            //        entidadPadre.Nombre = dataGridView1.Rows[i].Cells["Nombre"].Value.ToString();
            //        entidadPadre.Script = dataGridView1.Rows[i].Cells["Script"].Value.ToString();
            //        entidadPadre.DetalleEstatus = int.Parse(dataGridView1.Rows[i].Cells["DetalleEstatus"].Value.ToString());
            //        entidadPadre.DetalleEstatusString = dataGridView1.Rows[i].Cells["DetalleEstatusString"].Value.ToString();
            //        entidadPadre.Error = dataGridView1.Rows[i].Cells["Error"].Value.ToString();
            //        entidadPadre.Nuevo = bool.Parse(dataGridView1.Rows[i].Cells["Nuevo"].Value.ToString());
            //    }

            //    list.Add(entidadPadre);

            //}

            //dataGridView1.Columns.Clear();
            //dataGridView1.DataSource = list;

            //var editColumn = new DataGridViewButtonColumn
            //{
            //    Text = "Editar",
            //    UseColumnTextForButtonValue = true,
            //    Name = "Editar",
            //    DataPropertyName = "Editar"
            //};

            //dataGridView1.Columns.Add(editColumn);
            //dataGridView1.Columns["LiberacionID"].Visible = false;
            //dataGridView1.Columns["LiberacionEstatus"].Visible = false;
            //dataGridView1.Columns["LiberacionEstatusString"].Visible = false;
            //dataGridView1.Columns["Folio"].Visible = false;
            //dataGridView1.Columns["DetalleEstatus"].Visible = false;
            //dataGridView1.Columns["LiberacionDetalleID"].Visible = false;
            //dataGridView1.Columns["Nuevo"].Visible = false;
        }

        public void limpiar()
        {
            this.textBox1.Text = string.Empty;
            this.lblStatus.Text = string.Empty;
            this.textBox2.Text = string.Empty;
            this.dataGridView1.Columns.Clear();
            this.button1.Enabled = true;
            this.button2.Enabled = false;
            this.button3.Enabled = false;
            this.button4.Enabled = false;
            contador = 1;
            liberacionID = 0;
            progressBar1.Visible = false;
            this.checkBox1.Checked = false;
            this.checkBox2.Checked = false;
        }

        public void Buscar()
        {
            dataGridView1.Columns.Clear();
            DataSet Liberacion = new DataSet();
            DataSet Max = new DataSet();
            SqlConnection conn = new SqlConnection(conexionBase);

            try
            {
                //string query = "select MAX(LiberacionDetalleID) ultimo,L.LiberacionID,L.Folio,L.Estatus as LiberacionEstatus,LD.LiberacionDetalleID,LD.Nombre, LD.Script, LD.Estatus as DetalleEstatus, LD.Error, cast(L.UltActualizacion as int) UltActualizacion from Liberacion L inner join LiberacionDetalle LD on LD.liberacionID = L.LiberacionID where L.Folio =" + this.textBox1.Text + "group by L.LiberacionID,L.Folio,L.Estatus,LD.LiberacionDetalleID,LD.Nombre, LD.Script, LD.Estatus , LD.Error,L.UltActualizacion ";
                string query = "select L.LiberacionID,L.Descripcion,L.Folio,L.Estatus as LiberacionEstatus,LD.LiberacionDetalleID,LD.Nombre, LD.Script, LD.Estatus as DetalleEstatus, LD.Error, cast(L.UltActualizacion as int) UltActualizacion from Liberacion L inner join LiberacionDetalle LD on LD.liberacionID = L.LiberacionID where L.Folio =" + this.textBox1.Text + "Order by Nombre";

                SqlCommand sqlc = new SqlCommand(query, conn);
                sqlc.CommandType = CommandType.Text;
                conn.Open();
                sqlc.ExecuteNonQuery();
                SqlDataAdapter adapter = new SqlDataAdapter(sqlc);
                adapter.Fill(Liberacion, "Liberacion");

                sqlc = new SqlCommand("select MAX(LiberacionDetalleID) ultimo from Liberacion L inner join LiberacionDetalle LD on LD.liberacionID = L.LiberacionID where L.Folio =" + this.textBox1.Text, conn );
                sqlc.CommandType = CommandType.Text;
                sqlc.ExecuteNonQuery();
                adapter = new SqlDataAdapter(sqlc);
                adapter.Fill(Max, "Max");

                conn.Close();

                var LiberacionDetalle = (from d in Liberacion.Tables[0].AsEnumerable()
                                         select new
                                         {
                                             LiberacionID = d.Field<int>("LiberacionID"),
                                             Folio = d.Field<int>("Folio"),
                                             LiberacionEstatus = d.Field<int>("LiberacionEstatus"),
                                             LiberacionEstatusString = estatus.Activo.ToString(),
                                             LiberacionDetalleID = d.Field<int>("LiberacionDetalleID"),
                                             Nombre = d.Field<string>("Nombre"),
                                             Script = d.Field<string>("Script"),
                                             DetalleEstatus = d.Field<int>("DetalleEstatus"),
                                             DetalleEstatusString = estatus.Activo.ToString(),
                                             Error = d.Field<string>("Error"),
                                             UltActualizacion = int.Parse(d.Field<int>("UltActualizacion").ToString()),
                                             Nuevo = false   ,
                                             Modificado = false,
                                             Descripcion = d.Field<string>("Descripcion")
                                         }).ToList();

                dataGridView1.DataSource = LiberacionDetalle;

                var removeColumn = new DataGridViewCheckBoxColumn
                {
                    Name = "Eliminar",
                    DataPropertyName = "Eliminar"
                };

                var liberarColumn = new DataGridViewCheckBoxColumn
                {
                    Name = "Liberar",
                    DataPropertyName = "Liberar"                    
                };

                var editColumn = new DataGridViewButtonColumn
                {
                    Text = "Editar",
                    UseColumnTextForButtonValue = true,
                    Name = "Editar",
                    DataPropertyName = "Editar"                    
                };

                dataGridView1.Columns.Add(editColumn);
                dataGridView1.Columns.Add(removeColumn);
                dataGridView1.Columns.Add(liberarColumn);
                dataGridView1.Columns["LiberacionID"].Visible = false;
                dataGridView1.Columns["LiberacionEstatus"].Visible = false;
                dataGridView1.Columns["LiberacionEstatusString"].Visible = false;
                dataGridView1.Columns["Folio"].Visible = false;
                dataGridView1.Columns["DetalleEstatus"].Visible = false;
                dataGridView1.Columns["LiberacionDetalleID"].Visible = false;
                dataGridView1.Columns["DetalleEstatusString"].Visible = false;
                dataGridView1.Columns["Nuevo"].Visible = false;
                dataGridView1.Columns["UltActualizacion"].Visible = false;
                dataGridView1.Columns["Modificado"].Visible = false;
                dataGridView1.Columns["Descripcion"].Visible = false;

                if (LiberacionDetalle.Count > 0)
                {
                    var MaxID = (from d in Max.Tables[0].AsEnumerable()
                                 select new
                                 {
                                     MaxID = d.Field<int>("ultimo"),
                                 }).ToList();

                    switch (LiberacionDetalle[0].LiberacionEstatus)
                    {
                        case 1:
                            lblStatus.Text = "Activo";
                            lblStatus.ForeColor = Color.Teal;
                            break;
                        case 2:

                            lblStatus.Text = "Liberado";
                            lblStatus.ForeColor = Color.Red;
                            break;
                    }

                    //contador = (int.Parse(Liberacion.Tables[0].Rows[LiberacionDetalle.Count - 1]["ultimo"].ToString())) + 1;
                    this.contador = (MaxID[0].MaxID) + 1;                    
                    this.liberacionID = LiberacionDetalle[0].LiberacionID;
                    this.ultAct = LiberacionDetalle[0].UltActualizacion;
                    this.textBox2.Text = LiberacionDetalle[0].Descripcion;


                    this.button2.Enabled = true;
                    this.button3.Enabled = true;
                }
                else
                {
                    lblStatus.Text = "Nuevo";
                    lblStatus.ForeColor = Color.Green;

                    this.button2.Enabled = true;
                    this.liberacionID = 0;
                }

                this.button4.Enabled = false;
                this.textBox1.Focus();                
                this.checkBox1.Checked = false;
                this.checkBox2.Checked = false;
                this.progressBar1.Visible = false;

            }
            catch (Exception ex)
            {
                throw ex;
            } 
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dataGridView1.Columns["Editar"].Index) 
                return;

            int col = this.dataGridView1.CurrentCell.ColumnIndex;
            int row = this.dataGridView1.CurrentCell.RowIndex;

            Entidad entidadPadre = new Entidad();
            entidadPadre.LiberacionID = int.Parse(dataGridView1.Rows[row].Cells["LiberacionID"].Value.ToString());
            entidadPadre.Folio = int.Parse(dataGridView1.Rows[row].Cells["Folio"].Value.ToString());
            entidadPadre.LiberacionEstatus = int.Parse(dataGridView1.Rows[row].Cells["LiberacionEstatus"].Value.ToString());
            entidadPadre.LiberacionEstatusString = dataGridView1.Rows[row].Cells["LiberacionEstatusString"].Value.ToString();
            entidadPadre.LiberacionDetalleID = int.Parse(dataGridView1.Rows[row].Cells["LiberacionDetalleID"].Value.ToString());
            entidadPadre.Nombre = dataGridView1.Rows[row].Cells["Nombre"].Value.ToString();
            entidadPadre.Script = dataGridView1.Rows[row].Cells["Script"].Value.ToString();
            entidadPadre.DetalleEstatus = int.Parse(dataGridView1.Rows[row].Cells["DetalleEstatus"].Value.ToString());
            entidadPadre.DetalleEstatusString = dataGridView1.Rows[row].Cells["DetalleEstatusString"].Value.ToString();
            entidadPadre.Error = dataGridView1.Rows[row].Cells["Error"].Value.ToString();
            entidadPadre.Nuevo = bool.Parse(dataGridView1.Rows[row].Cells["Nuevo"].Value.ToString());
            entidadPadre.Modificado = true;
            entidadPadre.Descripcion = dataGridView1.Rows[row].Cells["Descripcion"].Value.ToString();

            AgregarEditar AEForm = new AgregarEditar();
            AEForm.entidad = entidadPadre;
            AEForm.ShowDialog();

            List<Entidad> list = new List<Entidad>();

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                entidadPadre = new Entidad();

                if (AEForm.entidad.LiberacionDetalleID.ToString() == dataGridView1.Rows[i].Cells["LiberacionDetalleID"].Value.ToString())
                {
                    entidadPadre.LiberacionID = AEForm.entidad.LiberacionID;
                    entidadPadre.Folio = AEForm.entidad.Folio;
                    entidadPadre.LiberacionEstatus = AEForm.entidad.LiberacionEstatus;
                    entidadPadre.LiberacionEstatusString = AEForm.entidad.LiberacionEstatusString;
                    entidadPadre.LiberacionDetalleID = AEForm.entidad.LiberacionDetalleID;
                    entidadPadre.Nombre = AEForm.entidad.Nombre;
                    entidadPadre.Script = AEForm.entidad.Script;
                    entidadPadre.DetalleEstatus = AEForm.entidad.DetalleEstatus;
                    entidadPadre.DetalleEstatusString = AEForm.entidad.DetalleEstatusString;
                    entidadPadre.Error = AEForm.entidad.Error;
                    entidadPadre.Nuevo = AEForm.entidad.Nuevo;
                    entidadPadre.Modificado = AEForm.entidad.Modificado;
                    entidadPadre.Descripcion = AEForm.entidad.Descripcion;
                }
                else
                {
                    entidadPadre.LiberacionID = int.Parse(dataGridView1.Rows[i].Cells["LiberacionID"].Value.ToString());
                    entidadPadre.Folio = int.Parse(dataGridView1.Rows[i].Cells["Folio"].Value.ToString());
                    entidadPadre.LiberacionEstatus = int.Parse(dataGridView1.Rows[i].Cells["LiberacionEstatus"].Value.ToString());
                    entidadPadre.LiberacionEstatusString = dataGridView1.Rows[i].Cells["LiberacionEstatusString"].Value.ToString();
                    entidadPadre.LiberacionDetalleID = int.Parse(dataGridView1.Rows[i].Cells["LiberacionDetalleID"].Value.ToString());
                    entidadPadre.Nombre = dataGridView1.Rows[i].Cells["Nombre"].Value.ToString();
                    entidadPadre.Script = dataGridView1.Rows[i].Cells["Script"].Value.ToString();
                    entidadPadre.DetalleEstatus = int.Parse(dataGridView1.Rows[i].Cells["DetalleEstatus"].Value.ToString());
                    entidadPadre.DetalleEstatusString = dataGridView1.Rows[i].Cells["DetalleEstatusString"].Value.ToString();
                    entidadPadre.Error = dataGridView1.Rows[i].Cells["Error"].Value.ToString();
                    entidadPadre.Nuevo = bool.Parse(dataGridView1.Rows[i].Cells["Nuevo"].Value.ToString());
                    entidadPadre.Modificado = bool.Parse(dataGridView1.Rows[i].Cells["Modificado"].Value.ToString());
                    entidadPadre.Descripcion = dataGridView1.Rows[i].Cells["Descripcion"].Value.ToString();
                }

                list.Add(entidadPadre);

            }

            dataGridView1.Columns.Clear();
            dataGridView1.DataSource = list;

            var removeColumn = new DataGridViewCheckBoxColumn
            {
                Name = "Eliminar",
                DataPropertyName = "Eliminar"
            };

            var liberarColumn = new DataGridViewCheckBoxColumn
            {
                Name = "Liberar",
                DataPropertyName = "Liberar"
            };

            var editColumn = new DataGridViewButtonColumn
            {
                Text = "Editar",
                UseColumnTextForButtonValue = true,
                Name = "Editar",
                DataPropertyName = "Editar"
            };

            dataGridView1.Columns.Add(editColumn);
            dataGridView1.Columns.Add(removeColumn);
            dataGridView1.Columns.Add(liberarColumn);
            dataGridView1.Columns["LiberacionID"].Visible = false;
            dataGridView1.Columns["LiberacionEstatus"].Visible = false;
            dataGridView1.Columns["LiberacionEstatusString"].Visible = false;
            dataGridView1.Columns["Folio"].Visible = false;
            dataGridView1.Columns["DetalleEstatus"].Visible = false;
            dataGridView1.Columns["LiberacionDetalleID"].Visible = false;
            dataGridView1.Columns["DetalleEstatusString"].Visible = false;
            dataGridView1.Columns["Nuevo"].Visible = false;
            dataGridView1.Columns["Modificado"].Visible = false;
            dataGridView1.Columns["Descripcion"].Visible = false;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }

            if (e.KeyChar == 13)
            {
                if (textBox1.Text != string.Empty){
                    Buscar();
                    //this.button4.Enabled = false;
                }
                else
                    MessageBox.Show("Indicar el numero de folio.");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell chk = (DataGridViewCheckBoxCell)row.Cells["Eliminar"];
                if (checkBox1.Checked)
                {
                    row.Cells["Eliminar"].Value = true;
                }
                else
                {
                    row.Cells["Eliminar"].Value = false;
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell chk = (DataGridViewCheckBoxCell)row.Cells["Liberar"];
                if (checkBox2.Checked)
                {
                    row.Cells["Liberar"].Value = true;
                }
                else
                {
                    row.Cells["Liberar"].Value = false;
                }
            }
        }

    }
}
