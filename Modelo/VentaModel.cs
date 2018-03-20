﻿using Entidad;
using Modelo.Recursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modelo
{
    public class VentaModel
    {//
        private WebService webService = new WebService();

        public void guardar()
        {
            //Comentario Primero
            //Comentario 2
        }

        public void modificar()
        {

        }

        public void eliminar()
        {

        }

        public async Task<RootObject<Venta>> ventas(int idSucursal, int idPuntoVenta, int idPersonal, string idEstado, int page, int items)
        {
            try
            {
                // www.lineatienda.com/services.php/ventas/sucursal/0/puntoventa/0/per/0/estado/todos/1/100
                RootObject<Venta> ventas = await webService.GET<RootObject<Venta>>("ventas", String.Format("sucursal/{0}/puntoventa/{1}/per/{2}/estado/{3}/{4}/{5}", idSucursal, idPuntoVenta, idPersonal, idEstado, page, items));
                return ventas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Venta>> ventasUltimas(int idPersonal, int idSucursal, int todos, int gerente)
        {
            try
            {
                // www.lineatienda.com/services.php/ventasultimas/3/1/1/1
                List<Venta> ventasultimas = await webService.GET<List<Venta>>("ventasultimas", String.Format("{0}/{1}/{2}/{3}", idPersonal, idSucursal, todos, gerente));
                return ventasultimas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<T>> ventasPorMes<T>()
        {
            try
            {
                // localhost:8080/admeli/xcore/services.php/ventaspormes
                List<T> list = await webService.GET<List<T>>("ventaspormes");
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task<Response> eliminar(Venta currentVenta)
        {
            throw new NotImplementedException();
        }

        public Task<Response> desactivar(Venta currentVenta)
        {
            throw new NotImplementedException();
        }
        public async Task<List<Venta_correlativo>> listarNroDocumentoVenta(int idTipoDocumento, int idPuntoVenta)
        {
            try
            {
                // localhost:8080/admeli/xcore/services.php/ventacorrelativo/tipodoc/4/pv/1
                List<Venta_correlativo> list = await webService.GET<List<Venta_correlativo>>("ventacorrelativo", String.Format("tipodoc/{0}/pv/{1}", idTipoDocumento, idPuntoVenta));

                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //ventacorrelativo/tipodoc/4/pv/1
    }
}
