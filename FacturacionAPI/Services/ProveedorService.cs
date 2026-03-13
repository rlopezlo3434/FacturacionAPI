using FacturacionAPI.Data;
using FacturacionAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace FacturacionAPI.Services
{
    public class ProveedorService
    {
        private readonly SistemaVentasDbContext _context;

        public ProveedorService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<List<Proveedor>> ObtenerTodos()
        {
            return await _context.Proveedores
                .OrderBy(x => x.RazonSocial)
                .ToListAsync();
        }

        public async Task<Proveedor?> ObtenerPorId(int id)
        {
            return await _context.Proveedores
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Proveedor> Crear(Proveedor proveedor)
        {
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return proveedor;
        }

        public async Task<Proveedor?> Actualizar(int id, Proveedor proveedor)
        {
            var existente = await _context.Proveedores.FindAsync(id);

            if (existente == null)
                return null;

            existente.Ruc = proveedor.Ruc;
            existente.RazonSocial = proveedor.RazonSocial;
            existente.Numero = proveedor.Numero;
            existente.Activo = proveedor.Activo;

            await _context.SaveChangesAsync();

            return existente;
        }

        public async Task<bool> Eliminar(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
                return false;

            _context.Proveedores.Remove(proveedor);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
