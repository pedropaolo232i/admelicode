﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Admeli.Configuracion.Nuevo;
using Entidad;
using Modelo;

namespace Admeli.Productos.Nuevo.PDetalle
{
    public partial class FormDescuentoNuevo : Form
    {
        private FormProductoNuevo formProductoNuevo;

        private Descuento currentDescuento { get; set; }
        private int currentIDDescuento { get; set; }
        private bool nuevo { get; set; }

        private SucursalModel sucursalModel = new SucursalModel();
        private GrupoClienteModel grupoClienteModel = new GrupoClienteModel();
        private ProductoModel productoModel = new ProductoModel();
        private DescuentoModel descuentoModel = new DescuentoModel();

        #region ====================== Constructor ======================
        public FormDescuentoNuevo()
        {
            InitializeComponent();
        }

        public FormDescuentoNuevo(FormProductoNuevo formProductoNuevo)
        {
            InitializeComponent();
            this.formProductoNuevo = formProductoNuevo;
            nuevo = true;
        }

        public FormDescuentoNuevo(FormProductoNuevo formProductoNuevo, Descuento currentDescuento)
        {
            InitializeComponent();
            this.formProductoNuevo = formProductoNuevo;
            this.currentDescuento = currentDescuento;
            nuevo = false;
        }
        #endregion

        #region ========================== Root load ==========================
        private void FormDescuentoNuevo_Load(object sender, EventArgs e)
        {
            this.reLoad();
        }

        private void reLoad()
        {
            dtpFechaInicio.Value = DateTime.Now;
            dtpFechaFin.Value = DateTime.Now;

            cargarProducto21();
            cargarGrupoCliente();
            cargarSucursales();
        }
        #endregion

        #region ================================ Loads ================================
        private async void cargarSucursales()
        {
            try
            {
                sucursalBindingSource.DataSource = await sucursalModel.sucursalesPrecio(formProductoNuevo.currentIDProducto);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void cargarGrupoCliente()
        {
            try
            {
                grupoClienteBindingSource.DataSource = await grupoClienteModel.gclientes21();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void cargarProducto21()
        {
            try
            {
                productoBindingSource.DataSource = await productoModel.productos21(formProductoNuevo.currentIDProducto);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region ====================== Form Add Registers ======================
        private void btnAddGrupoCliente_Click(object sender, EventArgs e)
        {
            FormGrupoClienteNuevo formGrupoCliente = new FormGrupoClienteNuevo();
            formGrupoCliente.ShowDialog();
        }

        private void btnAddSucursal_Click(object sender, EventArgs e)
        {
            FormSucursalNuevo formSucursal = new FormSucursalNuevo();
            formSucursal.ShowDialog();
        }
        #endregion

        #region ========================== SAVE AND UPDATE ===========================
        private void btnAceptar_Click(object sender, EventArgs e)
        {
            executeGuardar();
        }

        private async void executeGuardar()
        {
            if (!validarCampos()) return;
            try
            {
                crearObjetoSucursal();
                if (nuevo)
                {
                    currentDescuento = new Descuento();
                    
                    string dateFin = String.Format("{0:u}", dtpFechaFin.Value);
                    dateFin = dateFin.Substring(0, dateFin.Length - 1);

                    string dateInicio = String.Format("{0:u}", dtpFechaInicio.Value);
                    dateInicio = dateInicio.Substring(0, dateInicio.Length - 1);
                   

                    // asignar al decuento
                    currentDescuento.cantidadMaxima = textMaximaVenta.Text.Trim();
                    currentDescuento.cantidadMinima = textMinimaVenta.Text.Trim();
                    currentDescuento.codigo = textCodigo.Text.Trim();
                    currentDescuento.descuento = textDescuento.Text.Trim();
                    currentDescuento.estado = 1;
                    currentDescuento.fechaFin = dateFin;
                    currentDescuento.fechaInicio = dateInicio;
                    currentDescuento.idAfectoProducto = 0;
                    currentDescuento.tipo = "General";
                    currentDescuento.idGrupoCliente =(int) cbxGrupoCliente.SelectedValue;
                    currentDescuento.idSucursal = (int)cbxSucursal.SelectedValue;
                    currentDescuento.idPresentacion = formProductoNuevo.currentIDProducto;

                    Response response = await descuentoModel.guardar(currentDescuento);
                    MessageBox.Show(response.msj, "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    //Response response = await descuentoModel.modificar(currentDescuento);
                    Response response=null;
                    MessageBox.Show(response.msj, "Modificar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void crearObjetoSucursal()
        {
           /* currentAlternativa = new Alternativa();
            if (!nuevo) currentAlternativa.idAlternativa = currentIDAlternativa; // Llenar el id categoria cuando este en esdo modificar

            currentAlternativa.descripcionAlternativa = textValorVariante.Text;
            currentAlternativa.estado = 1;
            currentAlternativa.ordenPosicion = Convert.ToInt32(textPosicion.Text);
            currentAlternativa.seleccionado = chkEsSeleccionado.Checked;
            currentAlternativa.idVariante = this.idVariante;*/
        }

        private bool validarCampos()
        {
            if (textCodigo.Text == "")
            {
                errorProvider1.SetError(textCodigo, "Este campo esta vacía");
                textCodigo.Focus();
                return false;
            }
            errorProvider1.Clear();

            if (textMaximaVenta.Text == "")
            {
                errorProvider1.SetError(textMaximaVenta, "Este campo esta vacía");
                textMaximaVenta.Focus();
                return false;
            }
            errorProvider1.Clear();

            if (textMinimaVenta.Text == "")
            {
                errorProvider1.SetError(textMinimaVenta, "Este campo esta vacía");
                textMinimaVenta.Focus();
                return false;
            }
            errorProvider1.Clear();

            if (textDescuento.Text == "")
            {
                errorProvider1.SetError(textDescuento, "Este campo esta vacía");
                textDescuento.Focus();
                return false;
            }
            errorProvider1.Clear();
            return true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion


    }
}
