using DinkToPdf;
using DinkToPdf.Contracts;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Text;

namespace FacturacionAPI.Services
{
    public class PdfService
    {
        public readonly IConverter _converter;
        private readonly VehicleIntakeService _service;
        private readonly VehicleBudgetService _serviceVehicleBudget;

        public PdfService(IConverter converter, VehicleIntakeService service, VehicleBudgetService serviceVehicleBudget)
        {
            _converter = converter;
            _service = service;
            _serviceVehicleBudget = serviceVehicleBudget;
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

        private string GenerarCabecera(VehicleIntakeDetailDto data)
        {
            return $@"
<style>

.form-container {{
    width: 900px;
    border: 1px solid #ccc;
    padding: 15px;
    font-family: Arial, sans-serif;
    font-size: 13px;
}}

.row {{
    display: flex;
    gap: 20px;
    margin-bottom: 10px;
}}

.field {{
    flex: 1;
    display: flex;
    gap: 6px;
    align-items: center;
}}

.field.large {{
    flex: 2;
}}

.field.small {{
    flex: 0.5;
}}

.form-container label {{
    font-size: 12px;
    width: 100px;
    min-width: 100px;
}}

.label2 {{
    width: auto;
    min-width: auto;
}}

.line {{
    flex: 1;
    border-bottom: 1px solid #000;
    border-left: 1px solid #000;
    height: 18px;
    padding-left: 4px;
}}

.box {{
    border: 1px solid #000;
    padding: 5px;
}}

.value {{
    text-align: center;
    font-weight: bold;
}}

.red {{
    color: red;
}}

.inline {{
    display: flex;
    align-items: center;
    gap: 5px;
}}

</style>

<div class='form-container'>

    <div class='row'>
        <div class='field large'>
            <label>Nombre</label>
            <div class='line'>{data?.Client?.Names ?? ""}</div>
        </div>

        <div class='field small box'>
            <label class='label2'>O/T N°</label>
            <div class='value red'>{data?.Id}</div>
        </div>
    </div>

    <div class='row'>
        <div class='field large'>
            <label>Dirección</label>
            <div class='line'></div>
        </div>

        <div class='field small box'>
            <label class='label2'>Ref N°</label>
            <div class='value'></div>
        </div>
    </div>

    <div class='row'>
        <div class='field'>
            <label>Teléfonos</label>
            <div class='line'></div>
        </div>

        <div class='field'>
            <label class='label2'>DNI</label>
            <div class='line'></div>
        </div>

        <div class='field small box'>
            <label class='label2'>Placa</label>
            <div class='value'>{data?.Vehicle?.Plate ?? ""}</div>
        </div>
    </div>

    <div class='row'>
        <div class='field'>
            <label>E-mail</label>
            <div class='line'></div>
        </div>

        <div class='field inline'>
            <label class='label2'>Efectivo</label><input type='radio'>
            <label class='label2'>Tarjeta</label><input type='radio'>
            <label class='label2'>Crédito</label><input type='radio'>
        </div>
    </div>

    <div class='row'>
        <div class='field'>
            <label>R.U.C</label>
            <div class='line'></div>
        </div>

        <div class='field'>
            <label>Facturar a</label>
            <div class='line'></div>
        </div>
    </div>

    <div class='row'>
        <div class='field'>
            <label>Vehículo - Marca</label>
            <div class='line'>{data?.Vehicle?.Brand?.Name ?? ""}</div>
        </div>

        <div class='field'>
            <label>Modelo - Año</label>
            <div class='line'>{data?.Vehicle?.Model?.Name ?? ""}</div>
        </div>
    </div>

    <div class='row'>
        <div class='field'>
            <label>Color</label>
            <div class='line'></div>
        </div>

        <div class='field'>
            <label>Kilometraje</label>
            <div class='line'>{data?.MileageKm}</div>
        </div>
    </div>

    <div class='row'>
        <div class='field'>
            <label>VIN / Serie</label>
            <div class='line'></div>
        </div>

        <div class='field'>
            <label>Proforma N°</label>
            <div class='line'></div>
        </div>
    </div>

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
        height: 24px;
    }

    .desc-cell {
        padding: 3px 6px !important;
        font-size: 11px;
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

        private string GenerarHtml(VehicleBudgetDetailDto data)
        {
            //var bloqueInventario = GenerarBloqueInventario(data);
            var cabecera = GenerarCabeceraPresupuesto(data);
            var respuestos = GenerarRepuestosDemo(data);
            var servicios = GenerarServiciosDemo(data);
            var otros = GenerarOtrosDemo();
            var generarTotales = GenerarTotales(5346, 962, 6308);
            return $@"
                    <html>
                    <head><meta charset='utf-8'></head>
                    <body>
                    
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
            return @"
<style>

.repuestos-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 11px;
    margin-top: 10px;
}

/* HEADER PRINCIPAL (REPUETOS) */
.repuestos-title {
    background: #507FC2;
    color: white;
    text-align: center;
    font-weight: bold;
    border: 1px solid black !important;
}

/* HEADER COLUMNAS */
.repuestos-head th {
    border: 1px solid black;
    padding: 4px;
    text-align: center;
    font-weight: bold;
}

/* FILAS */
.repuestos-table td {
    padding: 4px;
    border-bottom: 1px solid #cfcfcf;
}

/* ALINEACIONES */
.center {
    text-align: center;
}

.right {
    text-align: right;
}

</style>

<table class='repuestos-table'>

<!-- TITULO -->
<tr>
    <td colspan='6' class='repuestos-title'>REPUESTOS</td>
</tr>

<!-- CABECERA -->
<tr class='repuestos-head'>
    <th>ITEM</th>
    <th>CANT</th>
    <th>UND</th>
    <th>DESCRIPCION</th>
    <th>P.UNIT</th>
    <th>SUB TOTAL</th>
</tr>

<!-- FILAS -->
<tr>
    <td class='center'>1</td>
    <td class='center'>1</td>
    <td class='center'>UND</td>
    <td>FILTRO DE ACEITE</td>
    <td class='right'>80.00</td>
    <td class='right'>80.00</td>
</tr>

<tr>
    <td class='center'>2</td>
    <td class='center'>1</td>
    <td class='center'>UND</td>
    <td>ARANDELA DE TAPON DE CARTER</td>
    <td class='right'>3.00</td>
    <td class='right'>3.00</td>
</tr>

<tr>
    <td class='center'>3</td>
    <td class='center'>1.5</td>
    <td class='center'>GLN</td>
    <td>ACEITE DE MOTOR 5W20 SINTETICO CASTROL</td>
    <td class='right'>245.00</td>
    <td class='right'>367.50</td>
</tr>

<tr>
    <td class='center'>4</td>
    <td class='center'>1</td>
    <td class='center'>UND</td>
    <td>FILTRO DE AIRE</td>
    <td class='right'>125.00</td>
    <td class='right'>125.00</td>
</tr>

<tr>
    <td class='center'>5</td>
    <td class='center'>1</td>
    <td class='center'>UND</td>
    <td>FILTRO DE CABINA</td>
    <td class='right'>160.00</td>
    <td class='right'>160.00</td>
</tr>

<tr>
    <td class='center'>6</td>
    <td class='center'>1</td>
    <td class='center'>UND</td>
    <td>SPRAY LIMPIAFRENOS SONAX</td>
    <td class='right'>20.00</td>
    <td class='right'>20.00</td>
</tr>

<tr>
    <td class='center'>7</td>
    <td class='center'>1</td>
    <td class='center'>UND</td>
    <td>SHAMPOO LIMPIAPARABRISAS SONAX</td>
    <td class='right'>15.00</td>
    <td class='right'>15.00</td>
</tr>

<tr>
    <td class='center'>8</td>
    <td class='center'>1</td>
    <td class='center'>UND</td>
    <td>BATERIA SECUNDARIA - ORIGINAL</td>
    <td class='right'>687.70</td>
    <td class='right'>687.70</td>
</tr>

<!-- SUBTOTAL -->
<tr>
    <td colspan='5' class='right'><b>Sub total</b></td>
    <td class='right'><b>1,458.20</b></td>
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
        private string GenerarServiciosDemo(VehicleBudgetDetailDto data)
        {
            //var items = data.Items ?? new List<ItemDto>();

            // 🔥 Agrupar por ServicePackageId (solo los que tienen)
            var grupos = data.Items
                .Where(i => i.ServicePackageId != null && i.Service != null)
                .GroupBy(i => i.ServicePackageId);

            var independientes = data.Items
                .Where(i => i.ServicePackageId == null)
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
            decimal totalGeneral = data.Items.Sum(x => x.TotalPrice);

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

        private string GenerarTotales(decimal subtotal, decimal igv, decimal total)
        {
            return $@"
<style>

.totales-table {{
    width: 100%;
    border-collapse: collapse;
    font-size: 11px;
    margin-top: 10px;
}}

/* FILAS */
.totales-table td {{
    padding: 6px;
}}

/* LABEL */
.totales-label {{
    font-weight: bold;
}}

/* BOX DERECHA */
.totales-box {{
    border: 1px solid black;
    text-align: center;
    font-weight: bold;
}}

/* HEADER */
.totales-head {{
    font-weight: bold;
    text-align: center;
}}

/* OBSERVACIONES */
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

.right {{
    text-align: right;
}}

</style>

<!-- TOTALES -->
<table class='totales-table'>

<tr>
    <td class='totales-label'>MONEDA</td>
    <td><b>SOLES</b></td>

    <td class='totales-head'>SUB TOTAL</td>
    <td class='totales-head'>IGV</td>
    <td class='totales-head'>TOTAL</td>
</tr>

<tr>
    <td></td>
    <td></td>

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
GARANTIA DE 06 MESES POR EVAPORADO NUEVO COLOCADO
</div>
";
        }

    }



}
