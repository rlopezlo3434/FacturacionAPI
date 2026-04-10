using DinkToPdf;
using DinkToPdf.Contracts;
using FacturacionAPI.Migrations;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml.Style;
using QRCoder;
using SixLabors.ImageSharp;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace FacturacionAPI.Services
{
    public class PdfService
    {
        public readonly IConverter _converter;
        private readonly VehicleIntakeService _service;
        private readonly VehicleBudgetService _serviceVehicleBudget;
        private readonly WorkOrderService _serviceWorkOrder;
        private readonly FacturacionService _serviceFacturacion;

        private readonly IWebHostEnvironment _env;
        public PdfService(IConverter converter, VehicleIntakeService service, VehicleBudgetService serviceVehicleBudget, 
            IWebHostEnvironment env, WorkOrderService serviceWorkOrder, FacturacionService serviceFacturacion)
        {
            _converter = converter;
            _service = service;
            _serviceVehicleBudget = serviceVehicleBudget;
            _env = env;
            _serviceWorkOrder = serviceWorkOrder;
            _serviceFacturacion = serviceFacturacion;
        }
        public async Task<byte[]> GenerarPdfTest(int id)
        {
            //var data = await _service.GetIntakeDetailAsync(id);
            var data = await _serviceVehicleBudget.GetBudgetDetailAsync(id);

            var html = GenerarHtml(data);

            //var html = GenerarPresupuestoDemo();



            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            PaperSize = PaperKind.A4
        },
                Objects = {
            new ObjectSettings {
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
            };

            return _converter.Convert(doc);
        }


        public async Task<byte[]> GenerarPdfInternamiento(int id)
        {
            var data = await _service.GetIntakeDetailAsync(id);

            var html = GenerarHtmlInternamiento(data);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings =
                {
                    PaperSize = PaperKind.A4
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            return _converter.Convert(doc);

        }

        public async Task<byte[]> GenerarPdfOrdenTrabajo(int id)
        {
            var data = await _serviceWorkOrder.GetWorkOrderDetailAsync(id);

            var data2 = await _serviceVehicleBudget.GetBudgetDetailTrabajoAsync(data.VehicleIntakeId);
            //var data3 = await _serviceVehicleBudget.GetBudgetDetailAsync(data2);

            var html = GenerarHtmlOrdenTrabajo(data, data2);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings =
                {
                    PaperSize = PaperKind.A4
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            return _converter.Convert(doc);
        }

        public async Task<byte[]> GenerarPdfFactura(int id)
        {

            var data = await _serviceFacturacion.ObtenerVentaDetalleAsync(id);
            var html = GenerarHtmlFactura(data);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings =
                {
                    PaperSize = PaperKind.A4
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            return _converter.Convert(doc);
        }

        private string GenerarCabecera(VehicleIntakeDetailDto data)
        {
            string telefono = data.Client?.Numbers?
                               .FirstOrDefault(n => n.IsPrimary)?.Number
                               ?? data.Client?.Numbers?.FirstOrDefault()?.Number
                               ?? "-";
            string direccion = data.Client?.Addresses?
                                .FirstOrDefault(a => a.IsPrimary)?.Address
                                ?? data.Client?.Addresses?.FirstOrDefault()?.Address
                                ?? "-";

            return $@"
<style>

body {{
    font-family: Arial;
    font-size: 12px;
}}

.container {{
    width: 100%;
}}

.main-table {{
    width: 100%;
    border-collapse: collapse;
}}

.left {{
    width: 70%;
    vertical-align: top;
    padding-right: 10px;
}}

.right {{
    width: 30%;
    vertical-align: top;
}}

.row {{
    display: table;
    width: 100%;
    margin-bottom: 5px;
}}

.label {{
    display: table-cell;
    width: 130px;
}}

.line {{
    display: table-cell;
    border-bottom: 1px solid #000;
    height: 18px;
}}

.box {{
    border: 1px solid #000;
    padding: 10px;
}}

.box-row {{
    margin-bottom: 10px;
}}

.big {{
    font-size: 16px;
    font-weight: bold;
}}

.header {{
    margin-bottom: 10px;
}}

</style>

<div class='container'>

    <table class='main-table'>
        <tr>

            <!-- IZQUIERDA -->
            <td class='left'>

                <div class='row'>
                    <div class='label'>Nombre</div>
                    <div class='line'>{data?.Client?.Names ?? ""}</div>
                </div>

                <div class='row'>
                    <div class='label'>Dirección</div>
                    <div class='line'>{direccion}</div>
                </div>

                <div class='row'>
                    <div class='label'>Teléfonos</div>
                    <div class='line'>{telefono}</div>
                </div>

                <div class='row'>
                    <div class='label'>E-mail</div>
                    <div class='line'>{data?.Client?.Email ?? ""}</div>
                </div>

                <div class='row'>
                    <div class='label'>Vehículo - Marca</div>
                    <div class='line'>{data?.Vehicle?.Brand?.Name ?? ""}</div>
                </div>

                <div class='row'>
                    <div class='label'>Modelo - Año</div>
                    <div class='line'>{data?.Vehicle?.Model?.Name ?? ""} - {data?.Vehicle?.Anio.ToString() ?? ""}</div>
                </div>

                <div class='row'>
                    <div class='label'> Color </div>
                    <div class='line'>{data?.Vehicle?.Color ?? ""}</div>
                </div>

                <div class='row'>
                    <div class='label'> Kilometraje </div>
                    <div class='line'>{data?.MileageKm}</div>
                </div>

                <div class='row'>
                    <div class='label'> VIN / Serie </div>
                    <div class='line' >{data?.Vehicle.SerieNumber ?? ""}</div>
                </div>

            </td>

            <td class='right'>
                <div class='box'>
                    <div class='box-row'>
                        <div> N° Internamiento </div>
                        <div class= 'line big'>{data?.Id}</div>
                    </div>

                    <div class='box-row'>
                        <div> Fecha </div>
                        <div class= 'line' >{data?.CreatedAt:dd / MM / yyyy}</div>
                    </div>

                    <div class='box-row'>
                        <div> Hora </div>
                        <div class='line'>{data?.CreatedAt:HH: mm}</div>
                    </div>
                </div>
            </td>
        </tr>
    </table>
</div>
";
        }

        private string GenerarBloqueInventario(VehicleIntakeDetailDto data)
        {
            var items = data.InventoryItems
                .OrderBy(x => x.Group)
                .ThenBy(x => x.Name)
                .ToList();

            int totalBlocks = 5;
            int rowsPerBlock = (int)Math.Ceiling(items.Count / (double)totalBlocks);

            var blocks = new List<List<VehicleIntakeInventoryDetailDto>>();

            for (int i = 0; i < totalBlocks; i++)
            {
                blocks.Add(items.Skip(i * rowsPerBlock).Take(rowsPerBlock).ToList());
            }

            int maxRows = blocks.Max(x => x.Count);

            var sb = new StringBuilder();

            sb.Append(@"
<style>
    .inventory-section {
        margin-top: 10px;
        font-family: Arial, sans-serif;
        font-size: 11px;
    }

    .inventory-header {
        width: 100%;
        border-collapse: collapse;
        margin-bottom: 2px;
    }

    .inventory-header td {
        border: 1px solid #b7b7b7;
        padding: 4px 6px;
        font-size: 11px;
    }

    .inventory-title {
        background: #2f3b63;
        color: white;
        font-weight: bold;
        width: 180px;
    }

    .legend-text {
        background: #ffffff;
        text-align: center;
        font-weight: bold;
    }

    .legend-mark-ok {
        width: 36px;
        text-align: center;
        font-weight: bold;
        background: #4caf50;
        color: white;
    }

    .legend-mark-bad {
        width: 36px;
        text-align: center;
        font-weight: bold;
        background: #d9534f;
        color: white;
    }

    .legend-mark-na {
        width: 36px;
        text-align: center;
        font-weight: bold;
        background: #d9b443;
        color: white;
    }

    .inventory-table {
        width: 100%;
        border-collapse: separate; 
    border-spacing: 4px;    
        table-layout: fixed;
    }

    .inventory-table td {
        padding: 0;
        height: 18px;
    }

    .desc-cell {
        padding: 1px 6px !important;
        font-size: 9px;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .mark-cell {
        width: 26px;
        text-align: center;
        font-size: 13px;
        font-weight: bold;
        color: #244aa5;
    }

    .inventory-table tr:nth-child(odd) td {
        background-color: #f2f2f2; /* gris claro */
    }

    .inventory-table tr:nth-child(even) td {
        background-color: #e4e4e4; /* gris un poco más oscuro */
    }
    .empty-cell {
        height: 24px;
    }
</style>

<div class='inventory-section'>

    <table class='inventory-header'>
        <tr>
            <td class='inventory-title'>Inventario</td>

            <td class='legend-text'>Conforme</td>
            <td class='legend-mark-ok'>✔</td>

            <td class='legend-text'>Averiado</td>
            <td class='legend-mark-bad'>X</td>

          
        </tr>
    </table>

    <table class='inventory-table'>
");
            //<td class='legend-text'>Falta / No aplica</td>
            //<td class='legend-mark-na'>O</td>
            for (int row = 0; row < maxRows; row++)
            {
                sb.Append("<tr>");

                for (int block = 0; block < totalBlocks; block++)
                {
                    if (row < blocks[block].Count)
                    {
                        var item = blocks[block][row];

                        // Por ahora solo tienes isPresent.
                        // Si está presente => ✔
                        // Si no está => vacío
                        //var mark = item.IsPresent ? "✔" : "✘";
                        var mark = item.IsPresent ? "✔" : "";


                        sb.Append($@"
<td class='desc-cell'>{System.Net.WebUtility.HtmlEncode(item.Name)}</td>
<td class='mark-cell'>{mark}</td>
");
                    }
                    else
                    {
                        sb.Append(@"
<td class='empty-cell'></td>
<td class='empty-cell'></td>
");
                    }
                }

                sb.Append("</tr>");
            }

            sb.Append(@"
    </table>
</div>
");

            return sb.ToString();
        }
        private string GenerarBloqueDiagramas(VehicleIntakeDetailDto data)
        {
            if (data?.ImagesDiagram == null || !data.ImagesDiagram.Any())
                return "";

            // 🔥 Rutas base
            var base1Path = Path.Combine(_env.WebRootPath, "Intakes", "base", "AUTO1.png");
            var base2Path = Path.Combine(_env.WebRootPath, "Intakes", "base", "AUTO2.png");

            var base1Url = $"file:///{base1Path.Replace("\\", "/")}";
            var base2Url = $"file:///{base2Path.Replace("\\", "/")}";

            var html = @"
    <style>
        .diagram-wrapper {
            display: table;
            width: 100%;
        }

        .diagram-row {
            display: table-row;
        }

        .diagram-item {
            display: table-cell;
            width: 50%;
            padding: 0 5px;
            vertical-align: top;
        }

        .diagram-container {
            position: relative;
            width: 100%;
            height: 95px;
        }

        .diagram-container img {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            object-fit: contain;
        }

        .overlay {
            opacity: 0.9;
        }
    </style>";

            html += "<h4>Marcar observaciones en la carrocería</h4>";

            var imagenes = data.ImagesDiagram
                .OrderBy(x => x.CreatedAt)
                .ToList();

            html += "<div class='diagram-wrapper'><div class='diagram-row'>";

            for (int i = 0; i < imagenes.Count; i++)
            {
                var img = imagenes[i];
                var url = img.MarkedImageUrl;

                // 🔥 convertir ruta si es local
                if (!url.StartsWith("http"))
                {
                    var fullPath = Path.Combine(_env.WebRootPath, url.TrimStart('/'));
                    url = $"file:///{fullPath.Replace("\\", "/")}";
                }

                var baseUrl = (i == 0) ? base1Url : base2Url;

                html += $@"
        <div class='diagram-item'>
            <div class='diagram-container'>
                <img src='{baseUrl}' />
                <img src='{url}' class='overlay' />
            </div>
        </div>";
            }

            html += "</div></div>"; // cerrar row + wrapper
            html += "</div>";

            return html;
        }

        private string GenerarBloqueServicios(VehicleIntakeDetailDto data)
        {

            var servicios = (data?.Services ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim().ToUpper())
        .ToList();

            var sb = new StringBuilder();

            sb.Append(@"
<style>
    .services-section {
        margin-top: 10px;
        font-family: Arial, sans-serif;
        font-size: 11px;
    }

    .services-header {
        width: 100%;
        border-collapse: collapse;
        margin-bottom: 2px;
    }

    .services-header td {
        border: 1px solid #b7b7b7;
        padding: 4px 6px;
        font-size: 11px;
    }

    .services-title {
        background: #2f3b63;
        color: white;
        font-weight: bold;
    }

    .services-table {
        width: 100%;
        border-collapse: separate;
        border-spacing: 4px;
        table-layout: fixed;
    }

    .services-table td {
        padding: 0;
        height: 18px;
    }

    .desc-cell {
        padding: 2px 6px !important;
        font-size: 10px;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .num-cell {
        width: 25px;
        text-align: center;
        font-weight: bold;
        color: #244aa5;
    }

    .services-table tr:nth-child(odd) td {
        background-color: #f2f2f2;
    }

    .services-table tr:nth-child(even) td {
        background-color: #e4e4e4;
    }

    .empty-cell {
        height: 18px;
    }
</style>

<div class='services-section'>

    <table class='services-header'>
        <tr>
            <td class='services-title'>
                Servicios solicitados (Inspección, mantenimiento, reparaciones)
            </td>
        </tr>
    </table>

    <table class='services-table'>
");

            int maxRows = 10; // 🔥 igual que formato físico

            for (int i = 0; i < maxRows; i++)
            {
                sb.Append("<tr>");

                if (i < servicios.Count)
                {
                    var servicio = System.Net.WebUtility.HtmlEncode(servicios[i]);

                    sb.Append($@"
<td class='desc-cell'>{servicio}</td>
");
                }
                else
                {
                    sb.Append(@"
<td class='empty-cell'></td>
");
                }

                sb.Append("</tr>");
            }

            sb.Append(@"
    </table>
</div>
");

            return sb.ToString();
        }

        private string GenerarBloqueConformidad(VehicleIntakeDetailDto data)
        {
            var sb = new StringBuilder();

            sb.Append(@"
<style>
    .conf-section {
        margin-top: 15px;
        font-family: Arial, sans-serif;
        font-size: 11px;
    }

    .conf-header {
        background: #2f3b63;
        color: white;
        font-weight: bold;
        padding: 5px;
        margin-bottom: 5px;
    }

    .conf-table {
        width: 100%;
        border-collapse: collapse;
        margin-bottom: 10px;
    }

    .conf-table td {
        padding: 5px;
        vertical-align: middle;
    }

    .line2 {
        border-bottom: 1px solid #000;
        width: 100%;
        height: 18px;
    }

    .box-terms {
        width: 100%;
        border: 1px solid #000;
        border-collapse: collapse;
    }

    .box-terms td {
        border: 1px solid #000;
        padding: 8px;
        font-size: 10px;
        text-align: justify;
        vertical-align: top;
    }
</style>

<div class='conf-section'>

    <div class='conf-header'>Conformidad - Cliente o Representante</div>

    <table class='conf-table'>
        <tr>

            <td style='width:10%;'>Fecha:</td>
            <td style='width:20%;'><div class='line2'>" + DateTime.Now.ToString("dd/MM/yyyy") + @"</div></td>

            <td style='width:10%;'>Nombre:</td>
            <td style='width:30%;'><div class='line2'></div></td>

            <td style='width:10%;'>Firma:</td>
            <td style='width:20%;'><div class='line2'></div></td>

        </tr>
    </table>

    <table class='box-terms'>
        <tr>
            <td style='width:50%;'>
                Con el presente autorizo los trabajos antes mencionados, así como los repuestos y otros materiales 
                necesarios para efectuarlo. Asimismo, autorizo a la empresa y/o empleados, a operar el vehículo en 
                las calles, carreteras u otros lugares para revisarlo y probarlo. Acepto, además las condiciones 
                detalladas a continuación:
            </td>

            <td style='width:50%;'>
                La empresa no se responsabiliza por la pérdida o deterioro de artículos u objetos no declarados 
                por el cliente, así como de daños que pudieran ocurrir en caso de incendio, robo o accidentes que 
                no estén bajo nuestro control. La demora en el pago de la obligación asumida por la reparación 
                devengará en intereses y gastos de guardianía diarios.
            </td>
        </tr>
    </table>

</div>
");

            return sb.ToString();
        }
        private string GenerarHtmlInternamiento(VehicleIntakeDetailDto data)
        {
            var bloqueInventario = GenerarBloqueInventario(data);
            var cabecera = GenerarCabecera(data);
            var bloqueImagenes = GenerarBloqueDiagramas(data);
            var bloqueServicios = GenerarBloqueServicios(data);
            var bloqueConformidad = GenerarBloqueConformidad(data);

            var imagePath = Path.Combine(_env.WebRootPath, "header_Internamiento.png");
            var imageUrl = $"file:///{imagePath.Replace("\\", "/")}";

            return $@"
                    <html>
                    <head><meta charset='utf-8'></head>
                    <style>
                        body {{
                            margin: 0;
                            font-family: Arial;
                        }}

                        .header {{
                            width: 100%;
                        }}

                        .header img {{
                            width: 100%;
                            height: auto;
                        }}
                    </style>
                    <body>
                    <div class='header'>
                        <img src='{imageUrl}' />
                    </div>
                    {cabecera}
                    {bloqueInventario}
                    {bloqueImagenes}
                    {bloqueServicios}
                    {bloqueConformidad}
                    </body>
                    </html>";
        }

        private string GenerarFacturaCabecera(VentaDetalleResponseDto data)
        {
            var tipoComprobante = (data?.Serie != null && data.Serie.StartsWith("B"))
    ? "BOLETA ELECTRONICA"
    : "FACTURA ELECTRONICA";

            return $@"
                    <style>

                    .row-container {{
                        width: 100%;
                        border-collapse: collapse;
                    }}

                    .row-container td {{
                        vertical-align: top;
                    }}

                    /* IZQUIERDA */
                    .cabecera-table {{
                        width: 100%;
                        border-collapse: collapse;
                        font-size: 11px;
                    }}

                    .cabecera-table td {{
                        border: 1px solid black;
                        padding: 6px;
                    }}

                    .label-box {{
                        background: #507FC2;
                        color: black;
                        font-weight: bold;
                        width: 120px;
                    }}

                    .label-box small {{
                        float: right;
                    }}

                    /* DERECHA */
                    .factura-box {{
                        width: 100%;
                        height: 100%;
                        border: 1px solid black;
                        text-align: center;
                        font-family: Arial;
                        font-weight: bold;
                    }}

                    .factura-box .linea {{
                        padding: 10px 5px;
                    }}

                    .factura-title {{
                        font-size: 18px;
                    }}

                    .factura-ruc {{
                        font-size: 18px;
                    }}

                    .factura-numero {{
                        font-size: 18px;
                    }}

                    .cabecera-table td {{white - space: nowrap;
                    }}
                    </style>

                    <table class='row-container'>
                        <tr>

                            <!-- IZQUIERDA -->
                            <td style='width: 65%; padding-right:5px;'>

                                <table class='cabecera-table'>

                                    <tr>
                                        <td class='label-box'>FECHA <small>:</small></td>
                                        <td>{data?.FechaEmision:dd/MM/yyyy}</td>

                                        <td class='label-box'>G/R <small>:</small></td>
                                        <td>-</td>
                                    </tr>

                                    <tr>
                                        <td class='label-box'>CLIENTE <small>:</small></td>
                                        <td colspan='3'>{data?.ClienteNombre}</td>
                                    </tr>

                                    <tr>
                                        <td class='label-box'>R.U.C. <small>:</small></td>
                                        <td>{data?.ClienteDocumento}</td>

                                        <td class='label-box'>MONEDA <small>:</small></td>
                                        <td>SOLES</td>
                                    </tr>

                                    <tr>
                                        <td class='label-box'>DIRECCIÓN <small>:</small></td>
                                        <td colspan='3'>{data?.Direccion}</td>
                                    </tr>
                                    <tr>
                                        <td class='label-box'>NRO PLACA <small>:</small></td>
                                        <td colspan='3'>{data?.Placa}</td>
                                    </tr>
                                </table>

                            </td>

                            <!-- DERECHA -->
                            <td style='width: 35%;'>

                                <table class='factura-box'>
                                    <tr>
                                        <td class='linea factura-title'>
                                            {tipoComprobante}
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class='linea factura-ruc'>
                                            RUC: 20501583732
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class='linea factura-numero'>
                                            {data?.Serie}-{data?.Numero}
                                        </td>
                                    </tr>
                                </table>

                            </td>

                        </tr>
                    </table>
                    <table class='cabecera-table' style='margin-top:5px;'>
                        <tr>
                            <td class='label-box'>FORMA DE PAGO <small>:</small></td>
                            <td>Contado</td>

                            <td class='label-box'>COND. VENTA <small>:</small></td>
                            <td>CONTADO</td>

                            <td class='label-box'>F. VENC. <small>:</small></td>
                            <td>{data?.FechaEmision:dd/MM/yyyy}</td>
                        </tr>
                    </table>
                    ";
        }
        public static string Convertir(decimal numero)
        {
            long entero = (long)Math.Floor(numero);
            int decimales = (int)((numero - entero) * 100);

            return $"{ConvertirEntero(entero)} CON {decimales:00} / 100 SOLES";
        }

        private static string ConvertirEntero(long numero)
        {
            if (numero == 0) return "CERO";
            if (numero < 0) return "MENOS " + ConvertirEntero(Math.Abs(numero));

            string letras = "";

            if ((numero / 1000000) > 0)
            {
                letras += ConvertirEntero(numero / 1000000) + " MILLONES ";
                numero %= 1000000;
            }

            if ((numero / 1000) > 0)
            {
                letras += (numero / 1000 == 1 ? "MIL " : ConvertirEntero(numero / 1000) + " MIL ");
                numero %= 1000;
            }

            if ((numero / 100) > 0)
            {
                switch (numero / 100)
                {
                    case 1: letras += (numero % 100 == 0) ? "CIEN " : "CIENTO "; break;
                    case 2: letras += "DOSCIENTOS "; break;
                    case 3: letras += "TRESCIENTOS "; break;
                    case 4: letras += "CUATROCIENTOS "; break;
                    case 5: letras += "QUINIENTOS "; break;
                    case 6: letras += "SEISCIENTOS "; break;
                    case 7: letras += "SETECIENTOS "; break;
                    case 8: letras += "OCHOCIENTOS "; break;
                    case 9: letras += "NOVECIENTOS "; break;
                }
                numero %= 100;
            }

            if (numero > 0)
            {
                if (numero <= 20)
                {
                    string[] unidades = { "", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE",
                                     "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS",
                                     "DIECISIETE", "DIECIOCHO", "DIECINUEVE", "VEINTE" };
                    letras += unidades[numero];
                }
                else if (numero < 30)
                {
                    letras += "VEINTI" + ConvertirEntero(numero - 20);
                }
                else
                {
                    string[] decenas = { "", "", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA",
                                     "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };

                    letras += decenas[numero / 10];

                    if ((numero % 10) > 0)
                        letras += " Y " + ConvertirEntero(numero % 10);
                }
            }

            return letras.Trim();
        }
        private string GenerarDetalleFactura(VentaDetalleResponseDto? data)
        {
            var filas = "";
            int index = 1;
            var qr = GenerarQrBase64(data);
            var totalEnLetras = Convertir(data.Total);
            // 🔹 FILAS REALES
            foreach (var item in data.Detalles)
            {
                filas += $@"
        <tr>
            <td class='center'>{index}</td>
            <td class='center'>{item.Cantidad:0.00}</td>
            <td class='center'>UNI</td>
            <td>{item.Descripcion.ToUpper()}</td>
            <td class='right'>{item.ValorUnitario:0.00}</td>
            <td class='right'>{item.Total:0.00}</td>
        </tr>";

                index++;
            }

            // 🔥 🔥 RELLENO DE FILAS (CLAVE)
            int totalFilasDeseadas = 30;
            int filasActuales = data.Detalles.Count;
            int filasFaltantes = totalFilasDeseadas - filasActuales;

            for (int i = 0; i < filasFaltantes; i++)
            {
                filas += @"
        <tr>
            <td>&nbsp;</td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
        </tr>";
            }

            return $@"
<style>

body {{
    font-family: Arial, sans-serif;
    font-size: 11px;
    line-height: 1.4;
}}

.detalle-table {{
    width: 100%;
    border-collapse: collapse;
    font-size: 11px;
    margin-top: 10px;
}}

.detalle-table th {{
    background: #507FC2;
    border: 1px solid black;
    padding: 5px;
    text-align: center;
    font-weight: bold;
}}

.detalle-table td {{
    border-left: 1px solid black;
    border-right: 1px solid black;
    padding: 4px;
}}

.detalle-table tr:last-child td {{
    border-bottom: 1px solid black; /* 🔥 cierre tabla */
}}

.detalle-footer {{
    border: 1px solid black;
    padding: 6px;
    font-size: 11px;
    margin-top: 5px;
    border-radius: 5px;
}}

.center {{ text-align: center; }}
.right {{ text-align: right; }}

.totales-box {{
    width: 100%;
    border-collapse: collapse;
    font-size: 11px;
}}

.totales-box td {{
    border: 1px solid black;
    padding: 5px;
}}

.totales-head {{
    background: #507FC2;
    text-align: center;
    font-weight: bold;
}}

</style>

<table class='detalle-table'>

<tr>
    <th>ITEM</th>
    <th>CANTIDAD</th>
    <th>UND. MEDIDA</th>
    <th>DESCRIPCIÓN</th>
    <th>VALOR UNITARIO</th>
    <th>IMPORTE</th>
</tr>

{filas}

<tr>
    <td></td>
    <td></td>
    <td></td>
    <td style='padding-top:10px;'>
        <b>MARCA:</b> FORD &nbsp;&nbsp;
        <b>MODELO:</b> RANGER<br/>
        <b>AÑO:</b> 2023
    </td>
    <td></td>
    <td></td>
</tr>

</table>

<!-- SON -->
<div class='detalle-footer'>
    SON: {totalEnLetras}
</div>

<!-- BLOQUE INFERIOR -->
<table style='width:100%; margin-top:10px; border-collapse:collapse; font-size:11px;'>
<tr>

<td style='width:55%; vertical-align:top;'>

    <div style='font-size:9px; margin-bottom:5px;'>
        AUTORIZADO MEDIANTE RESOLUCIÓN DE<br/>
        SUPERINTENDENCIA N° 155-2017/SUNAT - ANEXO IV
    </div>

    <img src='data:image/png;base64,{qr}' style='width:110px;' />

</td>

<td style='width:45%; vertical-align:top;'>

    <table class='totales-box'>
        <tr class='totales-head'>
            <td>SUB TOTAL</td>
            <td>IGV 18 %</td>
            <td>TOTAL VENTA</td>
        </tr>
        <tr style='text-align:center;'>
            <td>S/ {data.Subtotal:0.00}</td>
            <td>S/ {data.Igv:0.00}</td>
            <td><b>S/ {data.Total:0.00}</b></td>
        </tr>
    </table>

</td>

</tr>
</table>

<div style='border:1px solid black; padding:8px; font-size:11px; margin-top:5px;'>
    Esta es una representación impresa de la factura electrónica {data.Serie}-{data.Numero}. 
    Puede verificarla utilizando su clave SOL.
</div>

<table style='width:100%; border-collapse:collapse; font-size:11px; margin-top:5px;'>

<tr>
    <td colspan='4' style='background:#507FC2; border:1px solid black; text-align:center; font-weight:bold;'>
        NUESTRAS CUENTAS BANCARIAS
    </td>
</tr>

<tr>
    <td style='border:1px solid black;'><b>CTA CTE. BCP SOLES :</b></td>
    <td style='border:1px solid black; text-align:center;'>N° 194-19490840-16</td>
    <td style='border:1px solid black; text-align:center;'><b>CCI:</b> 002-194-001949084016-92</td>
</tr>

<tr>
    <td style='border:1px solid black;'><b>CTA CTE. BCP DOLARES :</b></td>
    <td style='border:1px solid black; text-align:center;'>N° 194-19411621-06</td>
    <td style='border:1px solid black; text-align:center;'><b>CCI:</b> 002-194-001941162106-93</td>
</tr>

<tr>
    <td colspan='4' style='background:#507FC2; border:1px solid black; text-align:center; font-weight:bold;'>
        BANCO DE LA NACION
    </td>
</tr>

<tr>
    <td style='border:1px solid black;'><b>CTA DETRACCION N° :</b></td>
    <td colspan='3' style='border:1px solid black; text-align:center;'>N° 00-076020744</td>
</tr>

</table>
";
        }
        public string GenerarQrBase64(VentaDetalleResponseDto venta)
        {
            string tipoComprobante = venta.TipoComprobante == "FACTURA" ? "01" : "03";
            string tipoDocCliente = venta.ClienteDocumento.Length == 11 ? "6" : "1";

            string qrTexto = $"{venta.ClienteDocumento}|{tipoComprobante}|{venta.Serie}|{venta.Numero:D8}|0|{venta.Igv:0.00}|{venta.Total:0.00}|{venta.FechaEmision:dd/MM/yyyy}|{tipoDocCliente}|{venta.ClienteDocumento}";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrData = qrGenerator.CreateQrCode(qrTexto, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode qrCode = new Base64QRCode(qrData);

            return qrCode.GetGraphic(5); // devuelve base64
        }
        private string GenerarHtmlFactura(VentaDetalleResponseDto? data)
        {
            var cabecera = GenerarFacturaCabecera(data);

            var imagePath = Path.Combine(_env.WebRootPath, "header_Factura.png");
            var imageUrl = $"file:///{imagePath.Replace("\\", "/")}";

            int filasPorPagina = 15;

            // 🔥 PAGINACIÓN
            var paginas = data.Detalles
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / filasPorPagina)
                .Select(g => g.Select(x => x.item).ToList())
                .ToList();

            string htmlPaginas = "";

            foreach (var itemsPagina in paginas)
            {
                htmlPaginas += $@"
        <div class='page'>

            <div class='header'>
                <img src='{imageUrl}' />
            </div>

            {cabecera}

            {GenerarDetalleFacturaPaginado(data, itemsPagina)}

        </div>";
            }

            return $@"
    <html>
    <head>
        <meta charset='utf-8'>
        <style>
            body {{
                margin: 0;
                font-family: Arial;
                line-height: 1.4;
                letter-spacing: 0.3px;
            }}

            .header {{
                width: 100%;
                margin-bottom: 10px;
            }}

            .header img {{
                width: 100%;
                height: auto;
            }}

            .page {{
                page-break-after: always;
            }}

            .page:last-child {{
                page-break-after: auto;
            }}

            .titulo-doc {{
                text-align: center;
                font-weight: bold;
                font-size: 2rem;
                margin: 15px 0;
            }}
        </style>
    </head>

    <body>
        {htmlPaginas}
    </body>

    </html>";
        }
        private string ObtenerCodigoDetraccion(int? tipo)
        {
            if (tipo == null)
                return "";

            var map = new Dictionary<int, string>
    {
        {1, "001"},
        {2, "002"},
        {3, "003"},
        {4, "004"},
        {5, "005"},
        {7, "007"},
        {8, "008"},
        {9, "009"},
        {10, "010"},
        {11, "011"},
        {12, "012"},
        {13, "014"},
        {14, "016"},
        {15, "017"},
        {17, "019"},
        {18, "020"},
        {19, "021"},
        {20, "022"},
        {21, "023"},
        {22, "024"},
        {23, "025"},
        {24, "026"},
        {25, "027"},
        {26, "028"},
        {28, "030"},
        {29, "031"},
        {30, "032"},
        {32, "034"},
        {33, "035"},
        {34, "036"},
        {35, "037"},
        {37, "039"},
        {38, "040"},
        {39, "041"},
        {40, "013"},
        {41, "015"},
        {42, "099"},
        {43, "044"},
        {44, "045"}
    };

            return map.ContainsKey(tipo.Value) ? map[tipo.Value] : "";
        }
        private string GenerarDetalleFacturaPaginado(
    VentaDetalleResponseDto data,
    List<VentaDetalleItemDto> items)
        {
            var filas = "";
            int index = 1;

            var htmlDetraccion = "";

            if (data.Serie != null && !data.Serie.StartsWith("B") && data.Detraccion)
            {
                htmlDetraccion = $@"
    <table style='width:100%; border-collapse:collapse; font-size:11px; margin-top:8px;'>
        <tr>
            <td colspan='5' style='border:1px solid black; background:#507FC2; color:white; font-weight:bold; text-align:center; padding:5px;'>
                Detalle de Detracciones:
            </td>
        </tr>

        <tr style='font-weight:bold; text-align:center;'>
            <td style='border:1px solid black;'>Cod,Bien o Servicio de Detracción</td>
            <td style='border:1px solid black;'>% de Detracción</td>
            <td style='border:1px solid black;'>Monto Detracción</td>
        </tr>

        <tr style='text-align:center;'>
            <td style='border:1px solid black;'>{ObtenerCodigoDetraccion(data.DetraccionTipo)}</td>
            <td style='border:1px solid black;'>{data.DetraccionPorcentaje:0.##}%</td>
            <td style='border:1px solid black;'>S/ {data.DetraccionMonto:0.00}</td>
        </tr>
    </table>";
            }

            foreach (var item in items)
            {
                filas += $@"
        <tr>
            <td class='center'>{index}</td>
            <td class='center'>{item.Cantidad:0.00}</td>
            <td class='center'>UNI</td>
            <td>{item.Descripcion.ToUpper()}</td>
            <td class='right'>{item.ValorUnitario:0.00}</td>
            <td class='right'>{item.Total:0.00}</td>
        </tr>";

                index++;
            }
            var totalEnLetras = Convertir(data.Total);

            int totalFilasDeseadas = data.Detraccion ? 23 : 26;
            int filasFaltantes = totalFilasDeseadas - items.Count;

            for (int i = 0; i < filasFaltantes; i++)
            {
                filas += @"
        <tr>
            <td>&nbsp;</td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
        </tr>";
            }

            var qr = GenerarQrBase64(data);

            return $@"
<style>
.detalle-table {{
    width: 100%;
    border-collapse: collapse;
    font-size: 11px;
    margin-top: 10px;
}}

.detalle-table th {{
    background: #507FC2;
    border: 1px solid black;
    padding: 5px;
    text-align: center;
    font-weight: bold;
}}

.detalle-table td {{
    border-left: 1px solid black;
    border-right: 1px solid black;
    padding: 4px;
    line-height: 1.4;
}}

.detalle-table tr:last-child td {{
    border-bottom: 1px solid black;
}}

.detalle-footer {{
    border: 1px solid black;
    padding: 6px;
    font-size: 11px;
    margin-top: 5px;
    border-radius: 5px;
}}

.center {{
    text-align: center;
}}

.right {{
    text-align: right;
}}

.totales-box {{
    width: 100%;
    border-collapse: collapse;
    font-size: 11px;
}}

.totales-box td {{
    border: 1px solid black;
    padding: 5px;
    text-align: center;
}}

.totales-head td {{
    background: #507FC2;
    font-weight: bold;
    text-align: center;
}}

.qr-text {{
    font-size: 9px;
    margin-bottom: 5px;
}}

.bancos-table {{
    width: 100%;
    border-collapse: collapse;
    font-size: 11px;
    margin-top: 5px;
}}

.bancos-table td {{
    border: 1px solid black;
    padding: 5px;
}}

.bancos-head {{
    background: #507FC2;
    text-align: center;
    font-weight: bold;
}}
</style>

<table class='detalle-table'>
    <tr>
        <th style='width:8%;'>ITEM</th>
        <th style='width:12%;'>CANTIDAD</th>
        <th style='width:12%;'>UND</th>
        <th style='width:40%;'>DESCRIPCIÓN</th>
        <th style='width:14%;'>VALOR</th>
        <th style='width:14%;'>IMPORTE</th>
    </tr>

    {filas}

    <tr>
        <td></td>
        <td></td>
        <td></td>
        <td style='padding-top:10px;'>
            <b>MARCA:</b> {data?.Marca}  &nbsp;&nbsp;
            <b>MODELO:</b> {data?.Modelo} <br/>
            <b>AÑO: {data?.Anio}</b> 
        </td>
        <td></td>
        <td></td>
    </tr>
</table>

<div class='detalle-footer'>
    SON: {totalEnLetras}
</div>

<table style='width:100%; margin-top:10px; border-collapse:collapse; font-size:11px;'>
    <tr>
        <td style='width:55%; vertical-align:top;'>
            <div class='qr-text'>
                AUTORIZADO MEDIANTE RESOLUCIÓN DE<br/>
                SUPERINTENDENCIA N° 155-2017/SUNAT
            </div>

            <img src='data:image/png;base64,{qr}' style='width:100px;' />
        </td>

        <td style='width:45%; vertical-align:top;'>
            <table class='totales-box'>
                <tr class='totales-head'>
                    <td>SUB TOTAL</td>
                    <td>IGV</td>
                    <td>TOTAL</td>
                </tr>
                <tr>
                    <td>S/ {data.Subtotal:0.00}</td>
                    <td>S/ {data.Igv:0.00}</td>
                    <td><b>S/ {data.Total:0.00}</b></td>
                </tr>
            </table>
        </td>
    </tr>
</table>

<div style='border:1px solid black; padding:8px; font-size:11px; margin-top:5px;'>
    Esta es una representación impresa de la factura electrónica {data.Serie}-{data.Numero}.
</div>

{htmlDetraccion}
<table class='bancos-table'>
    <tr>
        <td colspan='3' class='bancos-head'>
            NUESTRAS CUENTAS BANCARIAS
        </td>
    </tr>

    <tr>
        <td><b>CTA CTE. BCP SOLES :</b></td>
        <td style='text-align:center;'>194-19490840-16</td>
        <td style='text-align:center;'><b>CCI:</b> 002-194-001949084016-92</td>
    </tr>

    <tr>
        <td><b>CTA CTE. BCP DOLARES :</b></td>
        <td style='text-align:center;'>194-19411621-06</td>
        <td style='text-align:center;'><b>CCI:</b> 002-194-001941162106-93</td>
    </tr>

    <tr>
        <td colspan='3' class='bancos-head'>
            BANCO DE LA NACION
        </td>
    </tr>

    <tr>
        <td><b>CTA DETRACCION :</b></td>
        <td colspan='2' style='text-align:center;'>00-076020744</td>
    </tr>
</table>
";
        }

        private string GenerarHtmlOrdenTrabajo(WorkOrderDetailDto data, List<VehicleBudgetDetailDto> data2)
        {
            var cabecera = GenerarCabeceraPresupuesto(data2[0]);
            var respuestos = GenerarRepuestosDemo2(data, data2);
            var servicios = GenerarServiciosDemo2(data, data2);
            var generarTotales = GenerarTotales2(data, data2);

            var imagePath = Path.Combine(_env.WebRootPath, "header_Internamiento.png");
            var imageUrl = $"file:///{imagePath.Replace("\\", "/")}";

            return $@"
                    <html>
                    <head><meta charset='utf-8'></head>
                    <style>
                        body {{
                            margin: 0;
                            font-family: Arial;
                        }}

                        .header {{
                            width: 100%;
                        }}

                        .header img {{
                            width: 100%;
                            height: auto;
                        }}
                        .titulo-doc {{
                            text-align: center;
                            font-weight: bold;
                            font-size: 2rem;
                            margin: 15px 0;
                        }}
                    </style>
                    <body>
                    <div class='header'>
                        <img src='{imageUrl}' />
                    </div>
                    <div class='titulo-doc'>
                        ORDEN DE TRABAJO N° {data.Code}
                    </div>
                    {cabecera}
                    {respuestos}
                    {servicios}
                    {generarTotales}
                    </body>
                    </html>";
        }
        private string GenerarHtml(VehicleBudgetDetailDto data)
        {
            //var bloqueInventario = GenerarBloqueInventario(data);
            var cabecera = GenerarCabeceraPresupuesto(data);
            var respuestos = GenerarRepuestosDemo(data);
            var servicios = GenerarServiciosDemo(data);
            var otros = GenerarOtrosDemo();
            var generarTotales = GenerarTotales(data);

            var imagePath = Path.Combine(_env.WebRootPath, "header_Internamiento.png");
            var imageUrl = $"file:///{imagePath.Replace("\\", "/")}";

            return $@"
                    <html>
                    <head><meta charset='utf-8'></head>
                    <style>
                        body {{
                            margin: 0;
                            font-family: Arial;
                        }}

                        .header {{
                            width: 100%;
                        }}

                        .header img {{
                            width: 100%;
                            height: auto;
                        }}
                        .titulo-doc {{
                            text-align: center;
                            font-weight: bold;
                            font-size: 2rem;
                            margin: 15px 0;
                        }}
                    </style>
                    <body>
                    <div class='header'>
                        <img src='{imageUrl}' />
                    </div>
                    <div class='titulo-doc'>
                        PRESUPUESTO N° {data.Code}
                    </div>
                    {cabecera}
                    {respuestos}
                    {servicios}
                    {generarTotales}

                    </body>
                    </html>";
        }

        private string GenerarCabeceraPresupuesto(VehicleBudgetDetailDto data)
        {
            string telefono = data.VehicleIntake?.Client?.Numbers?
                                .FirstOrDefault(n => n.IsPrimary)?.Number
                                ?? data.VehicleIntake?.Client?.Numbers?.FirstOrDefault()?.Number
                                ?? "-";
            string direccion = data.VehicleIntake?.Client?.Addresses?
                                .FirstOrDefault(a => a.IsPrimary)?.AddressName
                                ?? data.VehicleIntake?.Client?.Addresses?.FirstOrDefault()?.AddressName
                                ?? "-";
            return $@"
                <style>

                .section {{
                    width: 100%;
                    border-collapse: collapse;
                    margin-top: 10px;
                    font-size: 11px;
                }}

                /* HEADER */
                .section th {{
                    background: #507FC2;
                    color: white;
                    text-align: center;
                    padding: 6px;
                    font-weight: bold;
                    border: 1px solid black;
                }}

                /* FILAS SIN BORDES */
                .section td {{
                    padding: 3px 4px;
                    border: none;
                }}

                /* LABELS */
                .label {{
                    font-weight: bold;
                    width: 110px;
                }}

                /* LÍNEA TIPO FORMULARIO */
                .line {{
                    border-bottom: 1px solid #000;
                    min-height: 16px;
                }}

                .section tr td:first-child {{
                    border-left: 1px solid black;
                }}

                .section tr td:last-child {{
                    border-right: 1px solid black;
                }}

                /* BORDE INFERIOR SOLO EN LA ÚLTIMA FILA */
                .section tr:last-child td {{
                    border-bottom: 1px solid black;
                }}
                .divider {{
                    border-right: 1px solid black !important;
                }}

                </style>

                <table class='section'>

                <!-- CABECERA -->
                <tr>
                    <th colspan='3'>DATOS DEL CLIENTE</th>
                    <th colspan='3'>DATOS DEL VEHÍCULO</th>
                </tr>

                <!-- FILA 1 -->
                <tr>
                    <td class='label'>CLIENTE</td>
                    <td>
                       {data.VehicleIntake.Client.Names}
                    </td>
                    <td class='divider'>
                        <b>DNI</b>
                       {data.VehicleIntake.Client.DocumentIdentificationNumber}
                    </td>

                    <td class='label'>PLACA</td>
                    <td colspan='2'>
                       {data.VehicleIntake.Vehicle.Plate}
                    </td>
                </tr>

                <!-- FILA 2 -->
                <tr>
                    <td class='label'>DIRECCIÓN</td>
                    <td class='divider' colspan='2'>
                       {direccion}
                    </td>

                    <td class='label'>MARCA / MODELO</td>
                    <td colspan='2'>{data.VehicleIntake.Vehicle.Brand.Name} / {data.VehicleIntake.Vehicle.Model.Name}</td>
                </tr>

                <!-- FILA 3 -->
                <tr>
                    <td class='label'>TELÉFONOS</td>
                    <td class='divider' colspan='2'>{telefono}</td>

                    <td class='label'>SERIE / VIN</td>
                    <td colspan='2'>{data.VehicleIntake.Vehicle.SerialNumber}</td>
                </tr>

                <!-- FILA 4 -->
                <tr>
                    <td class='label'>EMAIL</td>
                    <td class='divider' colspan='2'>{data.VehicleIntake.Client.Email}</td>

                    <td class='label'>AÑO</td>
                    <td colspan='2'>{data.VehicleIntake.Vehicle.Year}</td>
                </tr>

                <!-- FILA 5 -->
                <tr>
                    <td class='label'>FACTURAR A</td>
                    <td class='divider' colspan='2'></td>

                    <td class='label'>COLOR</td>
                    <td colspan='2'>{data.VehicleIntake.Vehicle.Color}</td>
                </tr>

                <!-- FILA 6 -->
                <tr>
                    <td class='label'>RUC</td>
                    <td class='divider' colspan='2'></td>

                    <td class='label'>KILOMETRAJE</td>
                    <td colspan='2'>{data.VehicleIntake.MileageKm}</td>
                </tr>

                </table>
                ";
        }

        private string GenerarRepuestosDemo(VehicleBudgetDetailDto data)
        {
            var repuestos = data.Items
                .Where(i => i.Product != null) // 🔥 Solo productos
                .ToList();

            int itemIndex = 1;
            string filas = "";

            foreach (var item in repuestos)
            {
                var descripcion = GetDescripcion(item);

                filas += $@"
        <tr>
            <td class='center'>{itemIndex}</td>
            <td class='center'>{item.Quantity}</td>
            <td class='center'>UND</td>
            <td>{descripcion}</td>
            <td class='right'>{item.UnitPrice:0.00}</td>
            <td class='right'>{item.TotalPrice:0.00}</td>
        </tr>";

                itemIndex++;
            }

            // 🔥 TOTAL
            decimal totalGeneral = repuestos.Sum(x => x.TotalPrice);

            return $@"
<style>

.repuestos-table {{
    width: 100%;
    border-collapse: collapse;
    font-size: 11px;
    margin-top: 10px;
}}

.repuestos-title {{
    background: #507FC2;
    color: white;
    text-align: center;
    font-weight: bold;
    border: 1px solid black !important;
}}

.repuestos-head th {{
    border: 1px solid black;
    padding: 4px;
    text-align: center;
    font-weight: bold;
}}

.repuestos-table td {{
    padding: 4px;
    border-bottom: 1px solid #cfcfcf;
}}

.center {{
    text-align: center;
}}

.right {{
    text-align: right;
}}

</style>

<table class='repuestos-table'>

<tr>
    <td colspan='6' class='repuestos-title'>REPUESTOS</td>
</tr>

<tr class='repuestos-head'>
    <th>ITEM</th>
    <th>CANT</th>
    <th>UND</th>
    <th>DESCRIPCION</th>
    <th>P.UNIT</th>
    <th>SUB TOTAL</th>
</tr>

{filas}

<tr>
    <td colspan='5' class='right'><b>Sub total</b></td>
    <td class='right'><b>{totalGeneral:0.00}</b></td>
</tr>

</table>
";
        }

        string GetDescripcion(VehicleBudgetItemDetailDto item)
        {
            if (item.Product != null)
                return item.Product?.Name ?? "PRODUCTO";


            return item.Service?.Name ?? "SERVICIO";

        }


        private string GenerarRepuestosDemo2(WorkOrderDetailDto data2, List<VehicleBudgetDetailDto> data)
        {

            // 🔥 1. Unir todos los items de todos los budgets
            var allItems = data
                .SelectMany(b => b.Items)
                .ToList();

            // 🔥 2. IDs usados en WorkOrder
            var budgetItemIds = data2.Items
                .Where(i => i.BudgetItemId != null)
                .Select(i => i.BudgetItemId!)
                .ToHashSet();

            // 🔥 3. Filtrar coincidencias (solo productos)
            var repuestos = allItems
                .Where(i => i.Product != null && budgetItemIds.Contains(i.Id))
                .ToList();
           
            int itemIndex = 1;
            string filas = "";

            foreach (var item in repuestos)
            {
                var descripcion = GetDescripcion(item);

                filas += $@"
        <tr>
            <td class='center'>{itemIndex}</td>
            <td class='center'>{item.Quantity}</td>
            <td class='center'>UND</td>
            <td>{descripcion}</td>
            <td class='right'>{item.UnitPrice:0.00}</td>
            <td class='right'>{item.TotalPrice:0.00}</td>
        </tr>";

                itemIndex++;
            }

            // 🔥 TOTAL
            decimal totalGeneral = repuestos.Sum(x => x.TotalPrice);

            return $@"
<style>

.repuestos-table {{
    width: 100%;
    border-collapse: collapse;
    font-size: 11px;
    margin-top: 10px;
}}

.repuestos-title {{
    background: #507FC2;
    color: white;
    text-align: center;
    font-weight: bold;
    border: 1px solid black !important;
}}

.repuestos-head th {{
    border: 1px solid black;
    padding: 4px;
    text-align: center;
    font-weight: bold;
}}

.repuestos-table td {{
    padding: 4px;
    border-bottom: 1px solid #cfcfcf;
}}

.center {{
    text-align: center;
}}

.right {{
    text-align: right;
}}

</style>

<table class='repuestos-table'>

<tr>
    <td colspan='6' class='repuestos-title'>REPUESTOS</td>
</tr>

<tr class='repuestos-head'>
    <th>ITEM</th>
    <th>CANT</th>
    <th>UND</th>
    <th>DESCRIPCION</th>
    <th>P.UNIT</th>
    <th>SUB TOTAL</th>
</tr>

{filas}

<tr>
    <td colspan='5' class='right'><b>Sub total</b></td>
    <td class='right'><b>{totalGeneral:0.00}</b></td>
</tr>

</table>
";
        }
    


        private string GenerarServiciosDemo(VehicleBudgetDetailDto data)
        {
            //var items = data.Items ?? new List<ItemDto>();

            // 🔥 Agrupar por ServicePackageId (solo los que tienen)
            var grupos = data.Items
                .Where(i => i.ServicePackageId != null && i.Service != null)
                .GroupBy(i => i.ServicePackageId);

            var independientes = data.Items
                .Where(i => i.ServicePackageId == null && i.Service != null)
                .ToList();

            int itemIndex = 1;
            string filas = "";

            foreach (var grupo in grupos)
            {
                var package = grupo.First().ServicePackage;

                string nombreGrupo = package?.Name ?? "SERVICIO";
                decimal totalGrupo = grupo.Sum(x => x.TotalPrice);

                // 🔹 Fila principal
                filas += $@"
                    <tr>
                        <td class='center'>{itemIndex}</td>
                        <td class='center'>1</td>
                        <td class='center'>UND</td>
                        <td class='servicio-main'>{nombreGrupo}</td>
                        <td></td>
                        <td class='right'>{totalGrupo:0.00}</td>
                    </tr>";

                // 🔹 Detalle
                foreach (var item in grupo)
                {
                    var descripcion = GetDescripcion(item);
                    filas += $@"
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td class='servicio-detalle'>{descripcion}</td>
                        <td></td>
                        <td></td>
                    </tr>";
                }

                itemIndex++;
            }

            foreach (var item in independientes)
            {
                var descripcion = GetDescripcion(item);

                filas += $@"
                <tr>
                    <td class='center'>{itemIndex}</td>
                    <td class='center'>{item.Quantity}</td>
                    <td class='center'>UND</td>
                    <td class='servicio-main'>{descripcion}</td>
                    <td class='right'></td>
                    <td class='right'>{item.TotalPrice:0.00}</td>
                </tr>";

                itemIndex++;
            }
            // 🔥 TOTAL GENERAL
            decimal totalGeneral = data.Items
                                        .Where(i => i.Service != null)
                                        .Sum(x => x.TotalPrice);

            return $@"
            <style>
            .servicios-table {{
                width: 100%;
                border-collapse: collapse;
                font-size: 11px;
                margin-top: 10px;
            }}

            .servicios-title {{
                background: #507FC2;
                color: white;
                text-align: center;
                font-weight: bold;
                border: 1px solid black !important;
            }}

            .servicios-head th {{
                border: 1px solid black;
                padding: 4px;
                text-align: center;
                font-weight: bold;
            }}

            .servicios-table td {{
                padding: 4px;
                border-bottom: 1px solid #dcdcdc;
            }}

            .center {{ text-align: center; }}
            .right {{ text-align: right; }}

            .servicio-main {{ font-weight: bold; }}
            .servicio-detalle {{
                padding-left: 40px;
                color: #444;
            }}
            </style>

            <table class='servicios-table'>

            <tr>
                <td colspan='6' class='servicios-title'>SERVICIOS</td>
            </tr>

            <tr class='servicios-head'>
                <th>ITEM</th>
                <th>CANT</th>
                <th>UND</th>
                <th>DESCRIPCION</th>
                <th>P.UNIT</th>
                <th>SUB TOTAL</th>
            </tr>

            {filas}

            <tr>
                <td colspan='5' class='right'><b>Sub total</b></td>
                <td class='right'><b>{totalGeneral:0.00}</b></td>
            </tr>

            </table>
            ";
        }

        private string GenerarServiciosDemo2(WorkOrderDetailDto data2, List<VehicleBudgetDetailDto> data)
        {
            // 🔥 1. Unir todos los items
            var allItems = data
                .SelectMany(b => b.Items)
                .ToList();

            // 🔥 2. Mapear WorkOrder (solo servicios)
            var workOrderMap = data2.Items
                .Where(i => i.ItemType == 2 && i.BudgetItemId != null)
                .GroupBy(i => i.BudgetItemId)
                .ToDictionary(
                    g => g.Key!,
                    g => g.Sum(x => x.Quantity)
                );

            // 🔥 3. Filtrar servicios usados
            var servicios = allItems
                .Where(i => i.Service != null && workOrderMap.ContainsKey(i.Id))
                .Select(i => new
                {
                    Item = i,
                    Quantity = workOrderMap[i.Id],
                    Total = workOrderMap[i.Id] * i.UnitPrice
                })
                .ToList();

            // 🔥 4. Agrupar por ServicePackage
            var grupos = servicios
                .Where(i => i.Item.ServicePackageId != null)
                .GroupBy(i => i.Item.ServicePackageId);

            var independientes = servicios
                .Where(i => i.Item.ServicePackageId == null)
                .ToList();

            int itemIndex = 1;
            string filas = "";

            // 🔥 GRUPOS
            foreach (var grupo in grupos)
            {
                var package = grupo.First().Item.ServicePackage;

                string nombreGrupo = package?.Name ?? "SERVICIO";
                decimal totalGrupo = grupo.Sum(x => x.Total);

                filas += $@"
        <tr>
            <td class='center'>{itemIndex}</td>
            <td class='center'>1</td>
            <td class='center'>UND</td>
            <td class='servicio-main'>{nombreGrupo}</td>
            <td></td>
            <td class='right'>{totalGrupo:0.00}</td>
        </tr>";

                foreach (var item in grupo)
                {
                    var descripcion = GetDescripcion(item.Item);

                    filas += $@"
            <tr>
                <td></td>
                <td></td>
                <td></td>
                <td class='servicio-detalle'>{descripcion}</td>
                <td></td>
                <td></td>
            </tr>";
                }

                itemIndex++;
            }

            // 🔥 INDEPENDIENTES
            foreach (var item in independientes)
            {
                var descripcion = GetDescripcion(item.Item);

                filas += $@"
        <tr>
            <td class='center'>{itemIndex}</td>
            <td class='center'>{item.Quantity}</td>
            <td class='center'>UND</td>
            <td class='servicio-main'>{descripcion}</td>
            <td></td>
            <td class='right'>{item.Total:0.00}</td>
        </tr>";

                itemIndex++;
            }

            // 🔥 TOTAL GENERAL REAL (WorkOrder)
            decimal totalGeneral = servicios.Sum(x => x.Total);

            return $@"
<style>
.servicios-table {{
    width: 100%;
    border-collapse: collapse;
    font-size: 11px;
    margin-top: 10px;
}}

.servicios-title {{
    background: #507FC2;
    color: white;
    text-align: center;
    font-weight: bold;
    border: 1px solid black !important;
}}

.servicios-head th {{
    border: 1px solid black;
    padding: 4px;
    text-align: center;
    font-weight: bold;
}}

.servicios-table td {{
    padding: 4px;
    border-bottom: 1px solid #dcdcdc;
}}

.center {{ text-align: center; }}
.right {{ text-align: right; }}

.servicio-main {{ font-weight: bold; }}
.servicio-detalle {{
    padding-left: 40px;
    color: #444;
}}
</style>

<table class='servicios-table'>

<tr>
    <td colspan='6' class='servicios-title'>SERVICIOS</td>
</tr>

<tr class='servicios-head'>
    <th>ITEM</th>
    <th>CANT</th>
    <th>UND</th>
    <th>DESCRIPCION</th>
    <th>P.UNIT</th>
    <th>SUB TOTAL</th>
</tr>

{filas}

<tr>
    <td colspan='5' class='right'><b>Sub total</b></td>
    <td class='right'><b>{totalGeneral:0.00}</b></td>
</tr>

</table>
";
        }

        private string GenerarOtrosDemo()
        {
            return @"
<style>

.otros-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 11px;
    margin-top: 10px;
}

/* TITULO */
.otros-title {
    background: #507FC2;
    color: white;
    text-align: center;
    font-weight: bold;
    border: 1px solid black !important;
}

/* CABECERA */
.otros-head th {
    border: 1px solid black;
    padding: 4px;
    text-align: center;
    font-weight: bold;
}

/* FILAS */
.otros-table td {
    padding: 4px;
    border-bottom: 1px solid #dcdcdc;
}

/* ALIGN */
.center { text-align: center; }
.right { text-align: right; }

</style>

<table class='otros-table'>

<!-- TITULO -->
<tr>
    <td colspan='6' class='otros-title'>OTROS</td>
</tr>

<!-- CABECERA -->
<tr class='otros-head'>
    <th>ITEM</th>
    <th>CANT</th>
    <th>UND</th>
    <th colspan='2'>DESCRIPCION</th>
    <th>SUB TOTAL</th>
</tr>

<!-- ITEMS -->
<tr>
    <td class='center'>1</td>
    <td class='center'>1</td>
    <td class='center'>UND</td>
    <td colspan='2'>MATERIALES DE TALLER (desengrasantes, solventes que no dañen las piezas de su auto)</td>
    <td class='right'>60.00</td>
</tr>

<tr>
    <td class='center'>2</td>
    <td class='center'>1</td>
    <td class='center'>UND</td>
    <td colspan='2'>LAVADO DE VEHICULO</td>
    <td class='right'>30.00</td>
</tr>

<tr>
    <td class='center'>3</td>
    <td class='center'>1</td>
    <td class='center'>UND</td>
    <td colspan='2'>MOVILIDAD DE TRAER REPUESTO ORIGINAL A TALLER</td>
    <td class='right'>28.00</td>
</tr>

<tr>
    <td class='center'>4</td>
    <td class='center'>1</td>
    <td class='center'>UND</td>
    <td colspan='2'>MOVILIDAD DE RECOJO Y ENTREGA DE VEHICULO A DOMICILIO</td>
    <td class='right'>40.00</td>
</tr>

<!-- SUBTOTAL -->
<tr>
    <td colspan='5' class='right'><b>Sub total</b></td>
    <td class='right'><b>158.00</b></td>
</tr>

</table>
";
        }


        private string GenerarTotales2(WorkOrderDetailDto data2, List<VehicleBudgetDetailDto> data)
        {
            var allItems = data.SelectMany(b => b.Items).ToList();

            var workOrderMap = data2.Items
                .Where(i => i.BudgetItemId != null)
                .GroupBy(i => i.BudgetItemId)
                .ToDictionary(g => g.Key!, g => g.Sum(x => x.Quantity));

            decimal subtotal = allItems
                .Where(i => workOrderMap.ContainsKey(i.Id))
                .Sum(i => workOrderMap[i.Id] * i.UnitPrice);

            decimal igv = subtotal * 0.18m;
            decimal total = subtotal + igv;

            var extrasLista = data
                .Where(x => !string.IsNullOrWhiteSpace(x.Extras))
                .SelectMany(x => x.Extras!
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim()))
                .Distinct()
                .ToList();

            var observacionesHtml = "";
            var notas = data2.Notes;
            if (!string.IsNullOrWhiteSpace(notas))
            {
                observacionesHtml = $@"
                <div class='obs-title'>OBSERVACIONES</div>

                <div class='obs-box'>
                    {notas}
                </div>";
            }

            return $@"
                    <style>

                    .totales-table {{
                        width: 100%;
                        border-collapse: collapse;
                        font-size: 11px;
                        margin-top: 10px;
                    }}

                    .totales-table td {{
                        padding: 6px;
                        border: 1px solid black; /* 🔥 BORDES EN TODO */
                    }}

                    .totales-label {{
                        font-weight: bold;
                        text-align: center;
                    }}

                    .totales-head {{
                        font-weight: bold;
                        text-align: center;
                    }}

                    .totales-box {{
                        text-align: center;
                        font-weight: bold;
                    }}

                    .obs-title {{
                        background: #507FC2;
                        color: white;
                        border: 1px solid black !important;
                        text-align: center;
                        font-weight: bold;
                        margin-top: 10px;
                    }}

                    .obs-box {{
                        padding: 8px;
                        border: 1px solid #ccc;
                        font-size: 11px;
                    }}

                    .obs-footer {{
                        margin-top: 8px;
                        font-size: 11px;
                    }}

                    </style>

                    <!-- TOTALES -->
                    <table class='totales-table'>

                    <tr>
                        <td class='totales-label'>MONEDA</td>
                        <td class='totales-head'>SUB TOTAL</td>
                        <td class='totales-head'>IGV</td>
                        <td class='totales-head'>TOTAL</td>
                    </tr>

                    <tr>
                        <td class='totales-box'>SOLES</td>
                        <td class='totales-box'>{subtotal:N2}</td>
                        <td class='totales-box'>{igv:N2}</td>
                        <td class='totales-box'>{total:N2}</td>
                    </tr>

                    </table>
                    {observacionesHtml}
                    ";
        }

        private string GenerarTotales(VehicleBudgetDetailDto data)
        {
            decimal subtotal = data.Items.Sum(x => x.TotalPrice);
            decimal igv = subtotal * 0.18m;
            decimal total = subtotal + igv;
            var extrasHtml = "";

            if (!string.IsNullOrWhiteSpace(data.Extras))
            {
                var extras = data.Extras
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim());

                extrasHtml = string.Join("", extras.Select(x => $"<div>• {x}</div>"));
            }
            return $@"
                    <style>

                    .totales-table {{
                        width: 100%;
                        border-collapse: collapse;
                        font-size: 11px;
                        margin-top: 10px;
                    }}

                    .totales-table td {{
                        padding: 6px;
                        border: 1px solid black; /* 🔥 BORDES EN TODO */
                    }}

                    .totales-label {{
                        font-weight: bold;
                        text-align: center;
                    }}

                    .totales-head {{
                        font-weight: bold;
                        text-align: center;
                    }}

                    .totales-box {{
                        text-align: center;
                        font-weight: bold;
                    }}

                    .obs-title {{
                        background: #507FC2;
                        color: white;
                        border: 1px solid black !important;
                        text-align: center;
                        font-weight: bold;
                        margin-top: 10px;
                    }}

                    .obs-box {{
                        background: yellow;
                        padding: 8px;
                        border: 1px solid #ccc;
                        font-size: 11px;
                    }}

                    .obs-footer {{
                        margin-top: 8px;
                        font-size: 11px;
                    }}

                    </style>

                    <!-- TOTALES -->
                    <table class='totales-table'>

                    <tr>
                        <td class='totales-label'>MONEDA</td>
                        <td class='totales-head'>SUB TOTAL</td>
                        <td class='totales-head'>IGV</td>
                        <td class='totales-head'>TOTAL</td>
                    </tr>

                    <tr>
                        <td class='totales-box'>SOLES</td>
                        <td class='totales-box'>{subtotal:N2}</td>
                        <td class='totales-box'>{igv:N2}</td>
                        <td class='totales-box'>{total:N2}</td>
                    </tr>

                    </table>

                    <!-- OBSERVACIONES -->
                    <div class='obs-title'>OBSERVACIONES</div>

                    <div class='obs-box'>
                    El presente documento <b>NO ESTA CERRADO AL 100%</b>, porque está sujeto a variaciones ya que pueden faltar cargos adicionales, ya sea en servicios o repuestos, los mismos que se pondrán en conocimiento del cliente
                    </div>

                    <div class='obs-footer'>
                    {extrasHtml}
                    </div>
                    ";
        }
    }
}
