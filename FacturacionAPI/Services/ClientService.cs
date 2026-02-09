using FacturacionAPI.Data;
using FacturacionAPI.Models.DTOs;
using FacturacionAPI.Models.Entities;
using FacturacionAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FacturacionAPI.Services
{
    public class ClientService
    {
        private readonly SistemaVentasDbContext _context;

        public ClientService(SistemaVentasDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ClientDto>> GetClientByEstablishment(int establishmentId)
        {
            return await _context.Client
                .Include(e => e.Numbers)
                .Include(e => e.Establishment)
                .Where(e => e.EstablishmentId == establishmentId)
                .Select(e => new ClientDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    DocumentIdentificationNumber = e.DocumentIdentificationNumber,
                    Email = e.Email,
                    Gender = e.Gender.ToString(),
                    DocumentIdentificationType = e.DocumentIdentificationType.ToString(),
                    IsActive = e.IsActive,
                    AcceptsMarketing = e.AcceptsMarketing,
                    Numbers = e.Numbers.Select(n => n.Number).ToList()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetChildrenByClient(int establishmentId)
        {
            return await _context.ChildrenClient
                .Include(e => e.Client)
                .Where(e => e.Client.EstablishmentId == establishmentId)
                .Select(e => new ChildrenClientDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    FechaCumpleanios = e.FechaCumpleanios,
                    Client = new ClientDto
                    {
                        Id = e.Client.Id,
                        FirstName = e.Client.FirstName,
                        LastName = e.Client.LastName,
                        DocumentIdentificationNumber = e.Client.DocumentIdentificationNumber,
                        Email = e.Client.Email,
                        Gender = e.Client.Gender.ToString(),
                        DocumentIdentificationType = e.Client.DocumentIdentificationType.ToString(),
                        IsActive = e.IsActive,
                    }
                })
                .ToListAsync();
        }
        public async Task<(bool Success, string Message)> UpdateClientAsync(int id, ClientCreateDto dto)
        {
            var client = await _context.Client.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null)
                return (false, "No se encontró el cliente en este establecimiento.");

            if (!string.IsNullOrWhiteSpace(dto.DocumentIdentificationNumber))
                client.DocumentIdentificationNumber = dto.DocumentIdentificationNumber;

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                client.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                client.LastName = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                client.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.DocumentIdentificationNumber))
                client.DocumentIdentificationNumber = dto.DocumentIdentificationNumber;

            if (dto.DocumentIdentificationType != null)
                client.DocumentIdentificationType = dto.DocumentIdentificationType;

            if (dto.Gender != null)
                client.Gender = dto.Gender;

            client.UpdatedAt = DateTime.Now;

            _context.Client.Update(client);
            await _context.SaveChangesAsync();

            return (true, "Cliente actualizado correctamente.");
        }

        public async Task<(bool Success, string Message)> CreateClientAsync(ClientCreateDto dto, int establishmentId)
        {
            var exists = await _context.Client.AnyAsync(c =>
                c.DocumentIdentificationNumber == dto.DocumentIdentificationNumber &&
                c.EstablishmentId == establishmentId);

            if (exists)
                return (false, "Ya existe un cliente con este documento en este establecimiento.");

            var client = new Client
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DocumentIdentificationType = dto.DocumentIdentificationType,
                DocumentIdentificationNumber = dto.DocumentIdentificationNumber,
                Email = dto.Email,
                Gender = dto.Gender,
                AcceptsMarketing = dto.AcceptsMarketing,
                EstablishmentId = establishmentId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Client.Add(client);
            await _context.SaveChangesAsync();

            if (dto.Numbers != null)
            {
                var validNumbers = dto.Numbers
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => new ClientNumbers
                    {
                        ClientId = client.Id,
                        Number = n.Trim()
                    })
                    .ToList();

                if (validNumbers.Any())
                {
                    _context.ClientNumbers.AddRange(validNumbers);
                    await _context.SaveChangesAsync();
                }
            }

            return (true, "Cliente creado correctamente.");
        }

        public async Task<(bool Success, string Message)> CreateClientNumber(int id, ClientNumbers dto)
        {
            var exists = await _context.ClientNumbers.AnyAsync(c =>
                c.Number == dto.Number);

            if (exists)
                return (false, "Ya existe el número en la BD.");

            var client = new ClientNumbers
            {
                ClientId = id,
                Number = dto.Number.Trim()
            };

            _context.ClientNumbers.Add(client);
            await _context.SaveChangesAsync();

            return (true, "Numero agregado correctamente.");
        }

        public async Task<(bool Success, string Message)> CreateChildrenClientAsync(ChildrenDto dto)
        {
            var parentClient = await _context.Client.FindAsync(dto.ClientId);
            if (parentClient == null)
                return (false, "No se encontró el cliente padre.");

            var child = new ChildrenClient
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                FechaCumpleanios = dto.FechaCumpleanios,
                ClientId = dto.ClientId,
                IsActive = true,
                Genero = dto.Genero
            };

            _context.ChildrenClient.Add(child);
            await _context.SaveChangesAsync();

            return (true, "Hijo agregado correctamente.");
        }

        public async Task<List<ChildrenDto>> ListChildrenClientAsync(int clientId)
        {
            var children = await _context.ChildrenClient
                    .Where(ch => ch.ClientId == clientId && ch.IsActive)
                    .Select(ch => new ChildrenDto
                    {
                        Id = ch.Id,
                        FirstName = ch.FirstName,
                        LastName = ch.LastName,
                        FechaCumpleanios = ch.FechaCumpleanios,
                        ClientId = ch.ClientId,
                        Genero = ch.Genero
                    })
                    .ToListAsync();

            return children;
        }

        public async Task<bool> UpdateChildAsync(UpdateChildrenDto dto)
        {
            var child = await _context.ChildrenClient
                .FirstOrDefaultAsync(c => c.Id == dto.Id && c.IsActive);

            if (child == null)
                return false;

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                child.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                child.LastName = dto.LastName;

            if (dto.FechaCumpleanios.HasValue)
                child.FechaCumpleanios = dto.FechaCumpleanios.Value;

            if (!string.IsNullOrWhiteSpace(dto.Genero))
                child.Genero = dto.Genero;

            _context.ChildrenClient.Update(child);
            await _context.SaveChangesAsync();

            return true;
        }



        public async Task<(bool Success, string Message)> UpdateChildrenClientAsync(int id, ChildrenDto dto)
        {
            var existingChild = await _context.ChildrenClient.FindAsync(id);
            if (existingChild == null)
                return (false, "No se encontró el hijo especificado.");

            // Solo actualiza si se envían valores no nulos
            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                existingChild.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                existingChild.LastName = dto.LastName;

            if (dto.FechaCumpleanios.HasValue)
                existingChild.FechaCumpleanios = dto.FechaCumpleanios.Value;

            // Si el DTO tiene IsActive y se desea permitir actualizarlo
            if (dto.IsActive != existingChild.IsActive)
                existingChild.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return (true, "Hijo actualizado correctamente.");
        }


        public async Task<(bool Success, string Message)> UpdateClientStateAsync(int id, bool estatus)
        {
            var client = await _context.Client.FirstOrDefaultAsync(e => e.Id == id);

            if (client == null)
                return (false, "Cliente no encontrado");

            client.IsActive = estatus;

            _context.Client.Update(client);

            await _context.SaveChangesAsync();

            return (true, "Cliente actualizado correctamente");
        }

        public async Task<(bool Success, string Message)> updateStateMarketing(int id, bool estatus)
        {
            var client = await _context.Client.FirstOrDefaultAsync(e => e.Id == id);

            if (client == null)
                return (false, "Cliente no encontrado");

            client.AcceptsMarketing = estatus;

            _context.Client.Update(client);

            await _context.SaveChangesAsync();

            return (true, "Promoción actualizado correctamente");
        }

        public async Task<int> ObtenerDescuentoPorCliente(string clienteDocumento)
        {
            var visitas = await _context.Ventas
                       .Where(v => v.ClienteDocumento == clienteDocumento
                                   && !v.IsAnnulled
                                   && !v.UsadoParaDescuento)
                       .CountAsync();

            var niveles = new List<(int visitas, int descuento)>
            {
                (3, 5),
                (6, 10),
                (9, 15),
                (12, 20) // este reinicia
            };

            int descuentoAplicado = 0;
            bool resetear = false;

            foreach (var n in niveles)
            {
                if (visitas + 1 == n.visitas)
                {
                    descuentoAplicado = n.descuento;

                    if (n.visitas == 12)
                        resetear = true;
                }
            }

            // Si llega a 20 ventas → resetea
            if (resetear)
            {
                await ResetearVisitasCliente(clienteDocumento);
                visitas = 0;
            }

            return descuentoAplicado;
        }


        private async Task ResetearVisitasCliente(string clienteDocumento)
        {
            var ventasCliente = await _context.Ventas
                .Where(v => v.ClienteDocumento == clienteDocumento && !v.IsAnnulled)
                .ToListAsync();

            // Marca todas como "no contables para visitas"
            ventasCliente.ForEach(v => v.UsadoParaDescuento = true);

            await _context.SaveChangesAsync();
        }

        public async Task<TarjetaClienteResponse> GetVisitaCliente(int clienteId)
        {
            var cliente = await _context.Client
                .FirstOrDefaultAsync(x => x.Id == clienteId);


            //var visitas = await _context.VisitaClientes
            //   .Where(x => x.ClienteId == clienteId &&
            //        x.CicloTarjeta == cliente.TarjetaCicloActual)
            //   .CountAsync();

            var visitas = await _context.VisitaClientes
                .Where(x => x.ClienteId == clienteId &&
                     x.CicloTarjeta == cliente.TarjetaCicloActual)
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            //// generar 12 casillas
            //var casillas = Enumerable.Range(1, 12)
            //    .Select(i => new CasillaDto
            //    {
            //        Id = i,
            //        Marcada = i <= visitas,
            //        Label = ObtenerLabelPorCasilla(i),
            //    })
            //    .ToList();

            var casillas = Enumerable.Range(1, 12)
                                .Select(i => new CasillaDto
                                {
                                    Id = i,
                                    Marcada = i <= visitas.Count,
                                    Label = ObtenerLabelPorCasilla(i),

                                    // 👇 fecha solo para casillas marcadas
                                    Fecha = i <= visitas.Count
                                        ? visitas[i - 1].Fecha
                                        : (DateTime?)null
                                })
                                .ToList();

            return new TarjetaClienteResponse
            {
                ClienteId = clienteId,
                TotalVisitas = visitas.Count,
                Casillas = casillas,
                DescuentoActual = CalcularDescuento(visitas.Count)
            };
        }

        private string? ObtenerLabelPorCasilla(int id)
        {
            return id switch
            {
                3 => "5% desc.",
                6 => "10% desc.",
                9 => "15% desc.",
                12 => "20% desc.",
                _ => null
            };
        }

        private int CalcularDescuento(int visitas)
        {
            return visitas switch
            {
                3 => 5,
                6 => 10,
                9 => 15,
                12 => 20,
                _ => 0
            };
        }

        public async Task<TarjetaClienteResponse> RegistrarVisitaCliente(int clienteId)
        {
            var cliente = await _context.Client
            .FirstOrDefaultAsync(x => x.Id == clienteId);

            if (cliente != null)
            {
                var visita = new VisitaCliente
                {
                    ClienteId = clienteId,
                    Fecha = DateTime.Now,
                    CicloTarjeta = cliente.TarjetaCicloActual
                };

                _context.VisitaClientes.Add(visita);
                await _context.SaveChangesAsync();
            }
            

            return await GetVisitaCliente(clienteId);
        }

        public async Task<TarjetaClienteResponse> ResetTarjeta(int clienteId)
        {
            var cliente = await _context.Client
                .FirstOrDefaultAsync(c => c.Id == clienteId);

            cliente.TarjetaCicloActual++;

            await _context.SaveChangesAsync();

            return await GetVisitaCliente(clienteId);
        }
    }
}
