﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Admeli.Compras.buscar;
using Admeli.Compras.Buscar;
using Entidad;
using Entidad.Configuracion;
using Modelo;
using Entidad.Location;

namespace Admeli.Compras.Nuevo
{
    public partial class FormOrdenComprarNuevo : Form
    {


        //variables para guardar todo la ordenCompra

        private CompraA compraA { get; set; }
        private PagoA pagoA { get; set; }
        private OrdenCompraA ordenCompraA { get; set; }
        private List<DetalleA> detalleA { get; set; }
        private OrdenCompraTotal compraTotal { get; set; }
        private List<OrdenCompraImpuesto> ordenCompraImpuestos;
        //

        private List<OrdenCompraModificar> ordenCompraModificar;

        private MonedaModel monedaModel = new MonedaModel();
        private TipoDocumentoModel tipoDocumentoModel = new TipoDocumentoModel();

        private DocCorrelativoModel docCorrelativoModel = new DocCorrelativoModel();
        private ProductoModel productoModel = new ProductoModel();
        private AlternativaModel alternativaModel = new AlternativaModel();
        private PresentacionModel presentacionModel = new PresentacionModel();
        private FechaModel fechaModel = new FechaModel();
        private CompraModel compraModel = new CompraModel();
        private OrdenCompraModel ordenCompraModel = new OrdenCompraModel();
        private MedioPagoModel medioPagoModel = new MedioPagoModel();
        private ImpuestoModel impuestoModel = new ImpuestoModel();

        private LocationModel locationModel = new LocationModel();
        /// Sus datos se cargan al abrir el formulario
        private List<Moneda> monedas { get; set; }
        private List<TipoDocumento> tipoDocumentos { get; set; }
        private FechaSistema fechaSistema { get; set; }
        private List<Producto> productos { get; set; }
        private List<MedioPago> medioPagos { get; set; }

        /// Llenan los datos en las interacciones en el formulario 
        private List<Presentacion> presentaciones { get; set; }
        private Producto currentProducto { get; set; }
        private Proveedor currentProveedor { get; set; }
        private Sucursal_correlativo sucursal_correlativo = new Sucursal_correlativo();
        /// Se preparan para realizar la compra de productos
        private List<DetalleCompra> detalleCompras { get; set; }
        private OrdenCompra currentOrdenCompra { get; set; } // notaEntrada,pago,pagoCompra
        private NotaEntrada currentNotaEntrada { get; set; }
        private Pago currentPago { get; set; }
        public PagoCompra currentPagoCompra { get; set; }
        public Sucursal idSucursal { get; set; }
        public Personal personal;
        public int nroNuevo=0;
        public UbicacionGeografica ubicacionGeografica;
        private bool nuevo { get; set; }

        private int idPresentacionDatagriview = 0;

        #region ============================ Constructor ============================
        public FormOrdenComprarNuevo(Sucursal idSucursal, Personal personal)
        {
            this.idSucursal = idSucursal;
            InitializeComponent();
            this.nuevo = true;
            cargarFechaSistema();
            compraA = new CompraA();
            pagoA = new PagoA();
            ordenCompraA = new OrdenCompraA();
            detalleA = new List<DetalleA>();
            compraTotal = new OrdenCompraTotal();
            this.personal = personal;
            

        }

        public FormOrdenComprarNuevo(OrdenCompra currentOrdenCompra, Sucursal idSucursal, Personal personal)
        {
            InitializeComponent();
            this.currentOrdenCompra = currentOrdenCompra;
            this.idSucursal = idSucursal;
            this.nuevo = false;
            compraA = new CompraA();
            pagoA = new PagoA();
            ordenCompraA = new OrdenCompraA();
            detalleA = new List<DetalleA>();
            compraTotal = new OrdenCompraTotal();
            this.personal = personal;

            if(currentOrdenCompra.estadoCompra!=8)
            {

                btnRealizarPago.Enabled = false;
            }
            
        }
        #endregion

        #region ============================== CRUDS ==============================
        private void btnBuscarProveedor_Click(object sender, EventArgs e)
        {
            executeBuscarProveedor();
        }
        #endregion

        #region ================================ Root Load ================================
        private void FormComprarNuevo_Load(object sender, EventArgs e)
        {
            if (nuevo == true)
            {
                this.reLoad();
                cargarCorrelactivo();
                cargarubigeoActual(idSucursal.idUbicacionGeografica);
            }
                

            else
            {
                this.reLoad();
                this.cargarOrden();
                cargarImpuesto();
                cargarubigeoActual(idSucursal.idUbicacionGeografica);
                cargarProductos();
                cargarProveedor();

                btnRealizarPago.Text = "Modificar orden";

            }

            AddButtonColumn();
         

        }

        private void AddButtonColumn()
        {
            DataGridViewButtonColumn buttons = new DataGridViewButtonColumn();
            {
                buttons.HeaderText = "Acciones";
                buttons.Text = "Eliminar";
                buttons.UseColumnTextForButtonValue = true;
                //buttons.AutoSizeMode =
                //   DataGridViewAutoSizeColumnMode.AllCells;
                buttons.FlatStyle = FlatStyle.Popup;
                buttons.CellTemplate.Style.BackColor = Color.Red;
                buttons.CellTemplate.Style.ForeColor = Color.White;
                
                buttons.Name = "acciones";

                //buttons.DisplayIndex = 0;
            }

            dataGridView.Columns.Add(buttons);

        }


        private void reLoad()
        {
            cargarMonedas();
            cargarTipoDocumento();
            cargarFechaSistema();
            cargarProductos();
            cargarMedioPago();

        }

        #endregion

        #region ============================== Load ==============================

        public void cargarProveedor()
        {

            textTipoCompra.Text = "ORDEN DE COMPRA";
            txtSerie.Text = currentOrdenCompra.serie;
            txtCorrelativo.Text = currentOrdenCompra.correlativo;
            textNombreProveedor.Text = currentOrdenCompra.nombreProveedor;
            textNombreProveedor.Enabled = false;
            textDireccion.Text = currentOrdenCompra.direccionProveedor;
            textDireccion.Enabled = false;
            dtpEmision.Value = currentOrdenCompra.plazoEntrega.date;
            // ver si compro esta orden de compra
            if (currentOrdenCompra != null)
                if (currentOrdenCompra.estadoCompra != 8)
                {
                    btnComprarOrden.Enabled = false;

                }



        }
        private async void cargarOrden()
        {

            ordenCompraModificar = await ordenCompraModel.dcomprasordencompra(currentOrdenCompra.idOrdenCompra);
            if (nuevo == false)
            {
                cargardatagriw();
            }

        }

        private void cargardatagriw()
        {
            DetalleCompra detalleCompra;
            if (detalleCompras == null) detalleCompras = new List<DetalleCompra>();
            foreach (OrdenCompraModificar o in ordenCompraModificar)
            {

                detalleCompra = new DetalleCompra();

                detalleCompra.cantidad = o.cantidad;
                detalleCompra.cantidadUnitaria = o.cantidadUnitaria;
                detalleCompra.codigoProducto = o.codigoProducto;
                detalleCompra.descripcion = o.descripcion;
                detalleCompra.descuento = o.descuento;
                detalleCompra.estado = o.estado;
                detalleCompra.idCombinacionAlternativa = o.idCombinacionAlternativa;
                detalleCompra.idCompra = o.idCompra;
                detalleCompra.idDetalleCompra = o.idDetalleCompra;
                detalleCompra.idPresentacion = o.idPresentacion;
                detalleCompra.idProducto = o.idProducto;
                detalleCompra.idSucursal = o.idSucursal;
                detalleCompra.nombreCombinacion = o.nombreCombinacion;
                detalleCompra.nombreMarca = o.nombreMarca;
                detalleCompra.nombrePresentacion = o.nombrePresentacion;
                detalleCompra.nro = o.nro;
                detalleCompra.precioUnitario = o.precioUnitario;
                detalleCompra.total = o.total;

                // agrgando un nuevo item a la lista
                detalleCompras.Add(detalleCompra);

                // Refrescando la tabla

            }
            detalleCompraBindingSource.DataSource = null;
            detalleCompraBindingSource.DataSource = detalleCompras;
            dataGridView.Refresh();
            // Calculo de totales y subtotales
            calculoSubtotal();
        }


        private async void cargarImpuesto()
        {

            ordenCompraImpuestos = await impuestoModel.impcompraproductoordencompra(currentOrdenCompra.idOrdenCompra, idSucursal.idSucursal);
        }

        private async void cargarubigeoActual(int idUbicacionGeografica)
        {
            ubicacionGeografica = await locationModel.ubigeoActual(idUbicacionGeografica);

            txtLugarEntrega.Text = ubicacionGeografica.nombreP + " - " + ubicacionGeografica.nombreN1 ;
            if (ubicacionGeografica.nombreN2 != "")
            {
                txtLugarEntrega.Text += " - " + ubicacionGeografica.nombreN2;
                if (ubicacionGeografica.nombreN3 != "")
                {
                    txtLugarEntrega.Text += " - " + ubicacionGeografica.nombreN3;

                }
            }


        }
       


        private async void cargarMonedas()
        {
            try
            {
                monedas = await monedaModel.monedas();
                cbxMoneda.DataSource = monedas;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void cargarTipoDocumento()
        {
            try
            {
                tipoDocumentos = await tipoDocumentoModel.tipoDocumentoVentas();
                //cbxTipoDocumento.DataSource = tipoDocumentos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void cargarFechaSistema()
        {
            try
            {
                if (!nuevo) return;
                fechaSistema = await fechaModel.fechaSistema();
                dtpEmision.Value = fechaSistema.fecha;
                // dtpPago.Value = fechaSistema.fecha;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void cargarProductos()
        {
            try
            {
                productos = await productoModel.productos();
                productoBindingSource.DataSource = productos;
                cbxCodigoProducto.SelectedIndex = -1;
                cbxDescripcion.SelectedIndex = -1;

               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            


        }

        private async void cargarMedioPago()
        {
            try
            {
                medioPagos = await medioPagoModel.medioPagos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private async void cargarCorrelactivo()
        {
            List<Sucursal_correlativo> list = await docCorrelativoModel.listarNroDocumentoSucursal(1, idSucursal.idSucursal);
            sucursal_correlativo = list[0];
            textTipoCompra.Text = "ORDEN DE COMPRA";
            txtSerie.Text = sucursal_correlativo.serie;
            txtCorrelativo.Text = sucursal_correlativo.correlativoActual;


            
        }
        #endregion

        #region ============================  Agregar Producto Al Carro ============================
        private void cbxCodigoProducto_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarProductoDetalle();
        }

        private void cbxDescripcion_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarProductoDetalle();
        }

        private void textCantidad_OnValueChanged(object sender, EventArgs e)
        {
            calcularTotal();
        }

        private void textDescuento_OnValueChanged(object sender, EventArgs e)
        {
            calcularTotal();
        }

        private void cbxUnidad_SelectedIndexChanged(object sender, EventArgs e)
        {
            calcularPrecioUnitario();
            calcularTotal();
        }

        private void btnAddCard_Click(object sender, EventArgs e)
        {
            if (detalleCompras == null) detalleCompras = new List<DetalleCompra>();
            DetalleCompra detalleCompra = new DetalleCompra();
            if (exitePresentacion(Convert.ToInt32(cbxPresentacion.SelectedValue)))
            {
                MessageBox.Show("Este dato ya fue agregado", "presentacion", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;

            }
            // Creando la lista
            detalleCompra.cantidad = double.Parse(textCantidad.Text.Trim(), CultureInfo.GetCultureInfo("en-US"));

         
            /// Busqueda presentacion
            Presentacion findPresentacion = presentaciones.Find(x => x.idPresentacion == Convert.ToInt32(cbxPresentacion.SelectedValue));
            detalleCompra.cantidadUnitaria = double.Parse(findPresentacion.cantidadUnitaria, CultureInfo.GetCultureInfo("en-US"));

            detalleCompra.codigoProducto = cbxCodigoProducto.Text.Trim();
            detalleCompra.descripcion = cbxDescripcion.Text.Trim();
            detalleCompra.descuento = Convert.ToDouble(textDescuento.Text.Trim());
            detalleCompra.estado = 1;
            detalleCompra.idCombinacionAlternativa = Convert.ToInt32(cbxCombinacion.SelectedValue);

            detalleCompra.idCompra = currentOrdenCompra!=null ? currentOrdenCompra.idCompra: 0;
            detalleCompra.idDetalleCompra =  0;
            detalleCompra.idPresentacion = Convert.ToInt32(cbxPresentacion.SelectedValue);

            detalleCompra.idProducto = Convert.ToInt32(cbxCodigoProducto.SelectedValue);
            detalleCompra.idSucursal = ConfigModel.sucursal.idSucursal;
            detalleCompra.nombreCombinacion = cbxCombinacion.Text;
            detalleCompra.nombreMarca = currentProducto.nombreMarca;
            detalleCompra.nombrePresentacion = cbxPresentacion.Text;
            detalleCompra.nro = 1;
            detalleCompra.precioUnitario = double.Parse(textPrecioUnidario.Text.Trim(), CultureInfo.GetCultureInfo("en-US"));
            detalleCompra.total = double.Parse(textTotal.Text.Trim(), CultureInfo.GetCultureInfo("en-US"));

            // agrgando un nuevo item a la lista
            detalleCompras.Add(detalleCompra);

            // Refrescando la tabla
            detalleCompraBindingSource.DataSource = null;
            detalleCompraBindingSource.DataSource = detalleCompras;
            dataGridView.Refresh();

            // Calculo de totales y subtotales
            calculoSubtotal();
        }



        private bool exitePresentacion(int idPresentacion )
        {
            foreach (DetalleCompra detalleCompra in detalleCompras)
            {
                if(detalleCompra.idPresentacion==idPresentacion)
                 return true;
            }

            return false;

        } 
        private void cargarProductoDetalle()
        {
            if (cbxCodigoProducto.SelectedIndex == -1 || cbxDescripcion.SelectedIndex == -1) return;
            try
            {
                /// Buscando el producto seleccionado
                int idProducto = Convert.ToInt32(cbxCodigoProducto.SelectedValue) | Convert.ToInt32(cbxDescripcion.SelectedValue);
                currentProducto = productos.Find(x => x.idProducto == idProducto);

                // Llenar los campos del producto escogido
                textCantidad.Text = "1";
                textDescuento.Text = "0";

                /// Cargando presentaciones
                cargarPresentaciones();

                /// Cargando alternativas del producto
                cargarAlternativas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void cargarPresentaciones()
        {
            if (cbxCodigoProducto.SelectedIndex == -1 || cbxDescripcion.SelectedIndex == -1) return; /// validacion
                                                                                                     /// Cargar las precentaciones
            string f = cbxCodigoProducto.SelectedValue.ToString();
            presentaciones = await presentacionModel.presentacionVentas(Convert.ToInt32(cbxCodigoProducto.SelectedValue));
            presentacionBindingSource.DataSource = presentaciones;
            //cbxPresentacion.SelectedIndex = -1;

            /// calculos
            calcularPrecioUnitario();
            calcularTotal();
        }

        private async void cargarAlternativas()
        {
            if (cbxCodigoProducto.SelectedIndex == -1 || cbxDescripcion.SelectedIndex == -1) return; /// validacion
                                                                                                     /// cargando las alternativas del producto
            List<AlternativaCombinacion> alternativaCombinacion = await alternativaModel.cAlternativa31(Convert.ToInt32(cbxCodigoProducto.SelectedValue));
            alternativaCombinacionBindingSource.DataSource = alternativaCombinacion;

            /// calculos
            calcularPrecioUnitario();
            calcularTotal();
        }

        private void calculoSubtotal()
        {
            double subtotal = 0;
            foreach (DetalleCompra item in detalleCompras)
            {
                subtotal += item.total;
            }

            textSubTotal.Text = subtotal.ToString();
            double impuesto = double.Parse(textImpuesto.Text, CultureInfo.GetCultureInfo("en-US"));
            textTotalNeto.Text = (subtotal + impuesto).ToString();
        }

        /// <summary>
        /// Calcular Total
        /// </summary>
        private void calcularTotal()
        {
            try
            {
                if (textCantidad.Text.Trim() == "") textTotal.Text = "0";
                if (textPrecioUnidario.Text.Trim() == "" || textCantidad.Text.Trim() == "" || textDescuento.Text.Trim() == "") return; /// Validación

                double precioUnidario = double.Parse(textPrecioUnidario.Text, CultureInfo.GetCultureInfo("en-US"));
                double cantidad = double.Parse(textCantidad.Text, CultureInfo.GetCultureInfo("en-US"));
                double descuento = double.Parse(textDescuento.Text, CultureInfo.GetCultureInfo("en-US"));
                double total = (precioUnidario * cantidad) - descuento;

                textTotal.Text = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0}", total);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Calcular total", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Calcular Precio Unitario
        /// </summary>
        private void calcularPrecioUnitario() {
            if (cbxCodigoProducto.SelectedIndex == -1 || cbxDescripcion.SelectedIndex == -1) return; /// Validación
            try
            {
                if (cbxPresentacion.SelectedIndex == -1)
                {
                    textPrecioUnidario.Text = currentProducto.precioCompra;
                }
                else
                {
                    // Buscar presentacion elegida
                    int idPresentacion = Convert.ToInt32(cbxPresentacion.SelectedValue);
                    Presentacion findPresentacion = presentaciones.Find(x => x.idPresentacion == idPresentacion);

                    // Realizando el calculo
                    double precioCompra = double.Parse(currentProducto.precioCompra, CultureInfo.GetCultureInfo("en-US"));
                    double cantidadUnitario = double.Parse(findPresentacion.cantidadUnitaria, CultureInfo.GetCultureInfo("en-US"));
                    double precioUnidatio = precioCompra * cantidadUnitario;

                    // Imprimiendo valor
                    textPrecioUnidario.Text = String.Format(CultureInfo.GetCultureInfo("en-US"), "{0}", precioUnidatio);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Calcular total", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        private void executeBuscarProveedor()
        {
            BuscarProveedor buscarProveedor = new BuscarProveedor();
            buscarProveedor.ShowDialog();
            currentProveedor = buscarProveedor.currentProveedor;
            mostrarProveedor();
        }

        private void mostrarProveedor()
        {
            if (currentProveedor != null)
            {
                textNombreProveedor.Text = currentProveedor.razonSocial;
                textDireccion.Text = currentProveedor.direccion;

            }
        }

        private void textNombreEmpresa_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            executeBuscarProveedor();
        }


        private void btnBuscarProducto_Click(object sender, EventArgs e)
        {
            BuscarProducto buscarProducto = new BuscarProducto();
            buscarProducto.ShowDialog();

        }

        private void cargarProductoSeleccionado()
        {
            if (currentProducto == null) return;
        }

        #region =============================== Realizar La Compra ===============================
        private async void btnRealizarCompra_Click(object sender, EventArgs e)
        {

            if (nroNuevo != 1)
            {
                //pago
                pagoA.estado = 1;// 8 si
                pagoA.estadoPago = 1;//ver que significado

                // Moneda aux = monedaBindingSource.;
                int i = cbxMoneda.SelectedIndex;
                //  Moneda aux = monedaBindingSource.List[i] as Moneda;
                pagoA.idMoneda = monedas[i].idMoneda;

                pagoA.idPago = currentOrdenCompra != null ? currentOrdenCompra.idPago : 0;
                pagoA.motivo = "COMPRA";
                pagoA.saldo = Convert.ToDouble(textTotalNeto.Text);
                pagoA.valorPagado = 0;
                pagoA.valorTotal = Convert.ToDouble(textTotalNeto.Text);
                // compra
                string date = String.Format("{0:u}", dtpEmision.Value);
                date = date.Substring(0, date.Length - 1);
                compraA.descuento = 0;//CAMBIAR SEGUN DATOS


                compraA.direccion = currentProveedor != null ? currentProveedor.direccion : currentOrdenCompra.direccionProveedor;
                compraA.direccionEntrega = txtDireccionEntrega.Text;
                compraA.estado = 8;//es una orden de compra que no ha sido asignado a una compra
                compraA.fechaFacturacion = date;
                compraA.fechaPago = date;
                compraA.formaPago = "EFECTIVO";
                compraA.idCompraValor = currentOrdenCompra != null ? currentOrdenCompra.idCompra : 0;

                compraA.idPersonal = personal.idPersonal;
                compraA.idProveedor = currentProveedor != null ? currentProveedor.idProveedor : currentOrdenCompra.idProveedor;
                compraA.idSucursal = idSucursal.idSucursal;
                compraA.idTipoDocumento = 1;// orden compra

                compraA.moneda = cbxMoneda.Text;// ver si es correcto

                compraA.numeroDocumento = "";//
                compraA.observacion = textObservacion.Text;
                compraA.plazoEntrega = date; // ver si es correcto
                compraA.rucDni = currentProveedor != null ? currentProveedor.ruc : currentOrdenCompra.rucDni;
                compraA.subTotal = Convert.ToDouble(textSubTotal.Text);
                compraA.tipoCambio = Convert.ToInt32(monedas[i].tipoCambio);
                compraA.tipoCompra = "Con productos";
                compraA.total = Convert.ToDouble(textTotalNeto.Text);
                compraA.ubicacion = txtLugarEntrega.Text;
                compraA.nombreProveedor = currentProveedor != null ? currentProveedor.razonSocial : currentOrdenCompra.nombreProveedor;



                //detalle


                foreach (DetalleCompra detalle in detalleCompras)
                {
                    DetalleA aux1 = new DetalleA();

                    aux1.cantidad = Convert.ToInt32(detalle.cantidad);
                    aux1.cantidadUnitaria = Convert.ToInt32(detalle.cantidadUnitaria);
                    aux1.codigoProducto = detalle.codigoProducto;
                    aux1.descripcion = detalle.descripcion;
                    aux1.descuento = detalle.descuento;
                    aux1.estado = detalle.estado;
                    aux1.idCombinacionAlternativa = detalle.idCombinacionAlternativa;
                    aux1.idCompra = currentOrdenCompra != null ? currentOrdenCompra.idCompra : 0; ;
                    aux1.idDetalleCompra = detalle.idDetalleCompra;
                    aux1.idPresentacion = detalle.idPresentacion;
                    aux1.idProducto = detalle.idProducto;
                    aux1.idSucursal = detalle.idSucursal;
                    aux1.nombreCombinacion = detalle.nombreCombinacion;
                    aux1.nombreMarca = detalle.nombreMarca;
                    aux1.nombrePresentacion = detalle.nombrePresentacion;
                    aux1.nro = detalle.nro;
                    aux1.precioUnitario = detalle.precioUnitario;
                    aux1.total = detalle.total;
                    detalleA.Add(aux1);


                }

                //orden de compra
                ordenCompraA.ubicacion = txtLugarEntrega.Text;
                ordenCompraA.total = Convert.ToDouble(textTotalNeto.Text);


                ordenCompraA.direccion = textDireccion.Text;
                ordenCompraA.estado = 1;
                ordenCompraA.direccionEntrega = txtDireccionEntrega.Text;
                ordenCompraA.moneda = cbxMoneda.Text;
                ordenCompraA.observacion = textObservacion.Text;
                ordenCompraA.tipoCambio = Convert.ToInt32(monedas[i].tipoCambio);
                ordenCompraA.formaPago = "EFECTIVO";
                ordenCompraA.nombreProveedor = textNombreProveedor.Text;
                ordenCompraA.rucDni = currentProveedor != null ? currentProveedor.ruc : currentOrdenCompra.rucDni;
                ordenCompraA.direccion = currentProveedor != null ? currentProveedor.direccion : currentOrdenCompra.direccionProveedor;
                ordenCompraA.plazoEntrega = date;
                ordenCompraA.idCompraValor = currentOrdenCompra != null ? currentOrdenCompra.idCompra : 0;
                ordenCompraA.numeroDocumento = "";
                ordenCompraA.idProveedor = currentProveedor != null ? currentProveedor.idProveedor : currentOrdenCompra.idProveedor;
                ordenCompraA.tipoCompra = "con productos";
                ordenCompraA.subTotal = Convert.ToDouble(textSubTotal.Text);

                ordenCompraA.estado = 1;
                ordenCompraA.idPersonal = personal.idPersonal;
                ordenCompraA.idTipoDocumento = 1;// orden compra
                ordenCompraA.idSucursal = idSucursal.idSucursal;
                ordenCompraA.fechaFacturacion = date;
                ordenCompraA.fechaPago = date;
                ordenCompraA.idUbicacionGeografica = ubicacionGeografica.idUbicacionGeografica;
                ordenCompraA.idOrdenCompra = currentOrdenCompra != null ? currentOrdenCompra.idOrdenCompra : 0;

                compraTotal.compra = compraA;
                compraTotal.detalle = detalleA;
                compraTotal.ordencompra = ordenCompraA;
                compraTotal.pago = pagoA;
                //

                await ordenCompraModel.guardar(compraTotal);


                if (nuevo)
                {
                    MessageBox.Show("Datos Guardados", "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    nroNuevo = 1;
                }
                else
                    MessageBox.Show("Datos  modificador", "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
                btnAddCard.Enabled = false;
        } 

        private void crearObjetoCompra()
        {
            Compra compra = new Compra();
          //  compra.descuento = textDescuentoCompra.Text;
            compra.direccion = textDireccion.Text;
            compra.estado = 1;
            // compra.fechaFacturacion = dtpEmision.Value.ToString("yyyy-MMM-dd  hh:mm:ss");
            //compra.fechaPago = "";
            compra.formaPago = "";

        }


        #endregion

        private void bunifuMetroTextbox1_OnValueChanged(object sender, EventArgs e)
        {

        }

        private void textTotal_OnValueChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            formGeografia lugar = new formGeografia();
            
            lugar.ShowDialog();
            txtLugarEntrega.Text = lugar.cadena;
           
            cargarubigeoActual(lugar.idUbicacionGeografia);


        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void textPrecioUnidario_OnValueChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private async void btnAddMarca_Click(object sender, EventArgs e)
        {



            if (currentOrdenCompra!=null)
            {

                CompraOrdenCompra compra = new CompraOrdenCompra();
                compra.estado = 1;// 
                compra.formaPago = "EFECTIVO";

                int i = cbxMoneda.SelectedIndex;
                compra.idMoneda = monedas[i].idMoneda;
                compra.idOrdenCompra = currentOrdenCompra.idOrdenCompra;
                compra.moneda= monedas[i].moneda;
                compra.subTotal = Convert.ToDouble(textSubTotal.Text);
                compra.tipoCambio= Convert.ToInt32(monedas[i].tipoCambio);
                compra.total= Convert.ToDouble(textTotalNeto.Text);               
                await ordenCompraModel.comprarOrdenCompra(compra);
                MessageBox.Show("Orden de compra realizada", "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnComprarOrden.Enabled = false;
                btnRealizarPago.Enabled = false;
            }
                
            else
                MessageBox.Show("no exite orden de compra", "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);


         


            return;


        }

        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verificando la existencia de datos en el datagridview
            if (dataGridView.Rows.Count == 0)
            {
                MessageBox.Show("No hay un registro seleccionado", "Modificar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            int index = dataGridView.CurrentRow.Index; // Identificando la fila actual del datagridview
            int idOrdenCompra = Convert.ToInt32(dataGridView.Rows[index].Cells[3].Value); // obteniedo el idRegistro del datagridview
            DetalleCompra aux = detalleCompras.Find(x => x.idPresentacion == idOrdenCompra); // Buscando la registro especifico en la lista de registros
            cbxCodigoProducto.Text = aux.codigoProducto;
            cbxDescripcion.Text = aux.descripcion;
            cbxCombinacion.Text = aux.nombreCombinacion;
            cbxPresentacion.Text = aux.nombrePresentacion;
            textCantidad.Text = Convert.ToString(aux.cantidad, CultureInfo.GetCultureInfo("en-US"));

           
            textPrecioUnidario.Text = Convert.ToString(aux.precioUnitario, CultureInfo.GetCultureInfo("en-US"));
            textDescuento.Text = Convert.ToString(aux.descuento, CultureInfo.GetCultureInfo("en-US"));
            textTotal.Text = Convert.ToString(aux.total, CultureInfo.GetCultureInfo("en-US"));
            idPresentacionDatagriview = idOrdenCompra;
        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            // Verificando la existencia de datos en el datagridview
            if (dataGridView.Rows.Count == 0)
            {
                MessageBox.Show("No hay un registro seleccionado", "Modificar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            int index = dataGridView.CurrentRow.Index; // Identificando la fila actual del datagridview
            int idOrdenCompra = Convert.ToInt32(dataGridView.Rows[index].Cells[3].Value); // obteniedo el idRegistro del datagridview
            DetalleCompra aux = detalleCompras.Find(x => x.idPresentacion == idOrdenCompra);
            aux.cantidad =Convert.ToDouble( textCantidad.Text);
            aux.precioUnitario = Convert.ToDouble(textPrecioUnidario.Text);
            aux.total = Convert.ToDouble(textTotal.Text);
            detalleCompraBindingSource.DataSource = null;
            detalleCompraBindingSource.DataSource = detalleCompras;
            dataGridView.Refresh();

            // Calculo de totales y subtotales
            calculoSubtotal();
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {

           
            int y = e.ColumnIndex;

            if (dataGridView.Columns[y].Name == "acciones")
            {
                if (dataGridView.Rows.Count == 0)
                {
                    MessageBox.Show("No hay un registro seleccionado", "Modificar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                int index = dataGridView.CurrentRow.Index; // Identificando la fila actual del datagridview
                int idOrdenCompra = Convert.ToInt32(dataGridView.Rows[index].Cells[3].Value); // obteniedo el idRegistro del datagridview
                DetalleCompra aux = detalleCompras.Find(x => x.idPresentacion == idOrdenCompra);

                dataGridView.Rows.RemoveAt(index);

                detalleCompras.Remove(aux);
            }


        }
    }
}
