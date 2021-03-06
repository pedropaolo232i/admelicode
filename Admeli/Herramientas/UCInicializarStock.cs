﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Admeli.Componentes;
using Modelo;
using Entidad;
using Admeli.Productos;
using Admeli.Productos.Nuevo;

namespace Admeli.Herramientas
{
    public partial class UCInicializarStock : UserControl
    {
        private FormPrincipal formPrincipal;
        public bool lisenerKeyEvents { get; set; }

        private CategoriaModel categoriaModel = new CategoriaModel();
        private SucursalModel sucursalModel = new SucursalModel();
        private AlmacenModel almacenModel = new AlmacenModel();
        private ProductoModel productoModel = new ProductoModel();

        private Paginacion paginacion;
        private List<ProductoData> productos { get; set; }
        private ProductoData currentProdcuto { get; set; }

        #region ================================ CONSTRUCTOR ================================
        public UCInicializarStock()
        {
            InitializeComponent();

            lblSpeedPages.Text = ConfigModel.configuracionGeneral.itemPorPagina.ToString();     // carganto los items por página
            paginacion = new Paginacion(Convert.ToInt32(lblCurrentPage.Text), Convert.ToInt32(lblSpeedPages.Text));
        }

        public UCInicializarStock(FormPrincipal formPrincipal)
        {
            InitializeComponent();
            this.formPrincipal = formPrincipal;

            lblSpeedPages.Text = ConfigModel.configuracionGeneral.itemPorPagina.ToString();     // carganto los items por página
            paginacion = new Paginacion(Convert.ToInt32(lblCurrentPage.Text), Convert.ToInt32(lblSpeedPages.Text));
        } 
        #endregion

        #region ================================= Paint =================================
        private void panelContainer_Paint(object sender, PaintEventArgs e)
        {
            DrawShape drawShape = new DrawShape();
            drawShape.lineBorder(panelContainer);
        }

        private void UCInicializarStock_Paint(object sender, PaintEventArgs e)
        {
            DrawShape drawShape = new DrawShape();
            drawShape.lineBorder(panelSucursal);
            drawShape.lineBorder(panelAlmacen);
        }
        #endregion

        #region ============================= ROOT LOAD =============================
        private void UCInicializarStock_Load(object sender, EventArgs e)
        {
            this.reLoad();

            // Preparando para los eventos de teclado
            this.ParentChanged += ParentChange; // Evetno que se dispara cuando el padre cambia // Este eveto se usa para desactivar lisener key events de este modulo
            if (TopLevelControl is Form) // Escuchando los eventos del formulario padre
            {
                (TopLevelControl as Form).KeyPreview = true;
                TopLevelControl.KeyUp += TopLevelControl_KeyUp;
            }
        }

        internal void reLoad(bool refreshData = true)
        {
            if (refreshData)
            {
                this.cargarCategorias(); // 
                this.cargarSucursales();
                this.cargarAlmacenes();
                this.cargarRegistros();
            }
            this.lisenerKeyEvents = true; // Active lisener key events
        }
        #endregion

        #region ======================== KEYBOARD ========================
        // Evento que se dispara cuando el padre cambia
        private void ParentChange(object sender, EventArgs e)
        {
            // cambiar la propiedad de lisenerKeyEvents de este modulo
            if (lisenerKeyEvents) lisenerKeyEvents = false;
        }

        // Escuchando los Eventos de teclado
        private void TopLevelControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (!lisenerKeyEvents) return;
            switch (e.KeyCode)
            {
                case Keys.F3:
                    //executeNuevo();
                    break;
                case Keys.F4:
                    executeModificar();
                    break;
                case Keys.F5:
                    cargarRegistros();
                    break;
                case Keys.F7:
                    //executeAnular();
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region ==================================== Load ====================================
        private async void cargarCategorias()
        {
            loadState(true);
            try
            {
                // Cargando las categorias desde el webservice
                List<Categoria> categoriaList = await categoriaModel.categoriasTodo();
                Categoria lastCategori = categoriaList.Last();//raiz
                categoriaList.Remove(lastCategori);

                // Cargando
                treeViewCategoria.Nodes.Clear(); // limpiando
                treeViewCategoria.Nodes.Add(lastCategori.idCategoria.ToString(), lastCategori.nombreCategoria); // Cargando categoria raiz


                List< TreeNode> listNode = new List<TreeNode>();
                
                foreach (Categoria categoria in categoriaList)
                {
                    TreeNode aux = new TreeNode(categoria.nombreCategoria);
                    aux.Name = categoria.idCategoria.ToString();
                    listNode.Add(aux);
                    
                }
                    treeviewVista(categoriaList, treeViewCategoria.Nodes[0],listNode);
                

                // Estableciendo valores por defecto
                /* if (ConfigModel.currentProductoCategory.Count > 0)
                 {
                     recuperarCatSeleccionado();
                 }
                 else
                 {
                     treeViewCategoria.Nodes[0].ExpandAll();
                 }
                 treeViewCategoria.Nodes[0].Checked = true;*/
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            loadState(false);
        }

        private Categoria primerElemento(List<Categoria> categoriaList)
        {

            Categoria lastCategori = categoriaList[0];//raiz
            categoriaList.Remove(lastCategori);
            return lastCategori;
        }
        private bool hijos(List<Categoria> categoriaList,Categoria padre)
        {
            bool tieneHijos = false;

            foreach (Categoria categoria in categoriaList)
            {
                if(categoria.padre == padre.nombreCategoria)
                {
                    tieneHijos = true;
                    break;

                }

            }
             return tieneHijos;
        }
        private TreeNode buscar(Categoria categoria,List<TreeNode> listNode)
        {
            TreeNode aux=new TreeNode();

            foreach( TreeNode node in listNode)
            {
                if (categoria.nombreCategoria==node.Text)
                {

                    aux = node;
                    break;
                }

            }

            return aux;

        }
        private TreeNode buscarPadre(Categoria categoria, List<TreeNode> listNode)
        {
            TreeNode aux = new TreeNode();

            foreach (TreeNode node in listNode)
            {
                if (categoria.padre==node.Text)
                {

                    aux = node;
                    break;
                }

            }

            return aux;

        }
        private void treeviewVista( List<Categoria> categoriaList,TreeNode padreNode, List<TreeNode> listNode)
        {
            if (categoriaList.Count== 0)
            {
                return;

            }
            else
            {
                    Categoria aux = primerElemento(categoriaList);
                    TreeNode auxTreeNode = buscar(aux, listNode);
                    TreeNode nodePadre = buscarPadre(aux, listNode);
                   if (hijos(categoriaList,aux))
                    {
                         auxTreeNode.ImageIndex = 1;
                    }

                    if (nodePadre.Text!="")
                    {
                        
                        nodePadre.Nodes.Add(auxTreeNode);
                        treeviewVista( categoriaList, padreNode, listNode);
                    }
                    else
                    {
                        
                        padreNode.Nodes.Add(auxTreeNode);
                        
                        treeviewVista(categoriaList, padreNode, listNode);

                    }
                        
                  
                
            }



        }
        

          
        private async void cargarSucursales()
        {
            // Cargando el combobox de personales
            loadState(true);
            try
            {
                cbxSucursales.DataSource = await sucursalModel.sucursales();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private async void cargarAlmacenes()
        {
            // Cargando el combobox de personales
            loadState(true);
            try
            {
                cbxAlmacenes.DataSource = await almacenModel.almacenes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void cargarRegistros()
        {
            loadState(true);
            try
            {
                Dictionary<string, int> list = new Dictionary<string, int>();
                list.Add("id0", 0);
                Dictionary<string, int> sendList = (ConfigModel.currentProductoCategory.Count == 0) ? list : ConfigModel.currentProductoCategory;

                int sucursalId = (cbxSucursales.SelectedIndex == -1) ? ConfigModel.sucursal.idSucursal : Convert.ToInt32(cbxSucursales.SelectedValue);
                int almacenID = (cbxAlmacenes.SelectedIndex == -1) ? ConfigModel.currentIdAlmacen : Convert.ToInt32(cbxAlmacenes.SelectedValue);

                RootObjectData rootObjectData = await productoModel.stockHerramienta<RootObjectData>(sendList, almacenID, sucursalId, paginacion.currentPage, paginacion.speed);

                
                // actualizando datos de páginacón
                paginacion.itemsCount = rootObjectData.nro_registros;
                paginacion.reload();

                // Ingresando
                productos = rootObjectData.productos;
                productoBindingSource.DataSource = productos;
                dataGridView.Refresh();

                // Mostrando la paginacion
                mostrarPaginado();

                // Formato de celdas

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                loadState(false);
            }
        }

        private async void cargarRegistrosBuscar()
        {
            loadState(true);
            try
            {
                if (textBuscar.Text == "")
                {
                    cargarRegistros();
                    return;
                }

                Dictionary<string, int> list = new Dictionary<string, int>();
                list.Add("id0", 0);
                Dictionary<string, int> sendList = (ConfigModel.currentProductoCategory.Count == 0) ? list : ConfigModel.currentProductoCategory;

                int sucursalId = (cbxSucursales.SelectedIndex == -1) ? ConfigModel.sucursal.idSucursal : Convert.ToInt32(cbxSucursales.SelectedValue);
                int almacenID = (cbxAlmacenes.SelectedIndex == -1) ? ConfigModel.currentIdAlmacen : Convert.ToInt32(cbxAlmacenes.SelectedValue);

                RootObjectData rootObjectData = await productoModel.stockHerramientaLike<RootObjectData>(sendList, textBuscar.Text, almacenID, sucursalId, paginacion.currentPage, paginacion.speed);


                // actualizando datos de páginacón
                paginacion.itemsCount = rootObjectData.nro_registros;
                paginacion.reload();

                // Ingresando
                productos = rootObjectData.productos;
                productoBindingSource.DataSource = productos;
                dataGridView.Refresh();

                // Mostrando la paginacion
                mostrarPaginado();

                // Formato de celdas

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Listar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                loadState(false);
            }
        }
        #endregion

        #region ===================== Eventos Páginación =====================
        private void mostrarPaginado()
        {
            lblCurrentPage.Text = paginacion.currentPage.ToString();
            lblPageAllItems.Text = String.Format("{0} Registros", paginacion.itemsCount.ToString());
            lblPageCount.Text = paginacion.pageCount.ToString();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (lblCurrentPage.Text != "1")
            {
                paginacion.previousPage();
                cargarRegistros();
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            if (lblCurrentPage.Text != "1")
            {
                paginacion.firstPage();
                cargarRegistros();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (lblPageCount.Text == "0") return;
            if (lblPageCount.Text != lblCurrentPage.Text)
            {
                paginacion.nextPage();
                cargarRegistros();
            }
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            if (lblPageCount.Text == "0") return;
            if (lblPageCount.Text != lblCurrentPage.Text)
            {
                paginacion.lastPage();
                cargarRegistros();
            }
        }

        private void lblSpeedPages_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                paginacion.speed = Convert.ToInt32(lblSpeedPages.Text);
                paginacion.currentPage = 1;
                cargarRegistros();
            }
        }

        private void lblCurrentPage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                paginacion.reloadPage(Convert.ToInt32(lblCurrentPage.Text));
                cargarRegistros();
            }
        }

        private void lblCurrentPage_KeyPress(object sender, KeyPressEventArgs e)
        {
            Validator.isNumber(e);
        }

        private void lblSpeedPages_KeyPress(object sender, KeyPressEventArgs e)
        {
            Validator.isNumber(e);
        }
        #endregion

        #region ============================= Estados =============================
        private void loadState(bool state)
        {
            formPrincipal.appLoadState(state);
            panelNavigation.Enabled = !state;
            dataGridView.Enabled = !state;
        }
        #endregion

        #region ============================ Tools Events ============================
        private void cbxSucursales_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarRegistros();
        }

        private void cbxAlmacenes_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarRegistros();
        }

        private void textBuscar_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                cargarRegistrosBuscar();
            }
        }

        private void btnIngresos_Click(object sender, EventArgs e)
        {
            cargarRegistros();
        }
        #endregion


        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            executeModificar();
        }

        private void btnNuevoCategoria_Click(object sender, EventArgs e)
        {

            FormCategoriaNuevo formCategoria = new FormCategoriaNuevo();
            formCategoria.ShowDialog();
            cargarCategorias();
        }

        private void btnActualizarCategoria_Click(object sender, EventArgs e)
        {
            cargarCategorias();
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            FormProductoNuevo producto = new FormProductoNuevo();
            producto.ShowDialog();
            cargarRegistros();

        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            cargarRegistros();
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            executeModificar();
        }

        private void executeModificar()
        {
            // Verificando la existencia de datos en el datagridview
            if (dataGridView.Rows.Count == 0)
            {
                MessageBox.Show("No hay un registro seleccionado", "Modificar", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            int index = dataGridView.CurrentRow.Index; // Identificando la fila actual del datagridview
            int idProducto = Convert.ToInt32(dataGridView.Rows[index].Cells[0].Value); // obteniedo el idRegistro del datagridview

            currentProdcuto = productos.Find(x => x.idProducto == idProducto); // Buscando la registro especifico en la lista de registros

            Producto auxProducto = new Producto();

            auxProducto.nombreProducto = currentProdcuto.nombreProducto;
            auxProducto.codigoProducto= currentProdcuto.codigoProducto;
            auxProducto.precioCompra= currentProdcuto.precioCompra;
            auxProducto.descripcionCorta=currentProdcuto.descripcionCorta;
            auxProducto.idProducto = currentProdcuto.idProducto;


            // Mostrando el formulario de modificacion
            FormProductoNuevo formProducto = new FormProductoNuevo(auxProducto);
            formProducto.ShowDialog();
            cargarRegistros(); // recargando loas registros en el datagridview
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {

        }
    }
}


public class RootObjectData
{
    public List<ProductoData> productos { get; set; }
    public List<object> combinacion { get; set; }
    public int nro_registros { get; set; }
}

public class ImpuestoData
{
    public int idImpuesto { get; set; }
    public string valorImpuesto { get; set; }
    public bool porcentual { get; set; }
}

public class ProductoData 
{
    public int idProducto { get; set; }
    public string codigoProducto { get; set; }
    public string nombreProducto { get; set; }
    public string descripcionCorta { get; set; }
    public string precioCompra { get; set; }
    public object precioConImpuesto { get; set; }
    public string utilidad { get; set; }
    public int estado { get; set; }
    public int idUnidadMedida { get; set; }
    public int idMarca { get; set; }
    public string nombreUnidad { get; set; }
    public string nombreMarca { get; set; }
    public bool enUso { get; set; }
    public bool mostrarWeb { get; set; }
    public bool mostrarPrecioWeb { get; set; }
    public string precioVenta { get; set; }
    public string stock { get; set; }
    public string stockFinanciero { get; set; }
    public int idSucursal { get; set; }
    public string sucursal { get; set; }
    public int idAlmacen { get; set; }
    public string almacen { get; set; }
    public string productoAlmacen { get; set; }
    public List<ImpuestoData> impuesto { get; set; }
}