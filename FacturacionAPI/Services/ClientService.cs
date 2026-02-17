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
                .Include(e => e.Addresses)
                .Include(e => e.Establishment)
                .Where(e => e.EstablishmentId == establishmentId)
                .Select(e => new ClientDto
                {
                    Id = e.Id,
                    Names = e.Names,
                    DocumentIdentificationNumber = e.DocumentIdentificationNumber,
                    Email = e.Email,

                    Gender = new CatalogItemDto
                    {
                        Id = (int)e.Gender,
                        Name = e.Gender.ToString()
                    },

                    DocumentIdentificationType = new CatalogItemDto
                    {
                        Id = (int)e.DocumentIdentificationType,
                        Name = e.DocumentIdentificationType.ToString()
                    },

                    IsActive = e.IsActive,
                    AcceptsMarketing = e.AcceptsMarketing,

                    Numbers = e.Numbers
                        .OrderByDescending(c => c.IsPrimary)
                        .Select(c => new ClientContactDto
                        {
                            Id = c.Id,
                            Number = c.Number,
                            ContactName = c.ContactName,
                            Type = (int)c.Type,
                            IsPrimary = c.IsPrimary
                        }).ToList(),

                    Addresses = e.Addresses
                        .OrderByDescending(c => c.IsPrimary)
                        .Select(c => new ClientAddresses
                        {
                            Id = c.Id,
                            Address = c.Address,
                            AddressName = c.AddressName,
                            IsPrimary= c.IsPrimary
                        }).ToList()
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
                        Names = e.Client.Names,
                        DocumentIdentificationNumber = e.Client.DocumentIdentificationNumber,
                        Email = e.Client.Email,
                        //Gender = e.Client.Gender.ToString(),
                        //DocumentIdentificationType = e.Client.DocumentIdentificationType.ToString(),
                        IsActive = e.IsActive,
                    }
                })
                .ToListAsync();
        }
        public async Task<(bool Success, string Message)> UpdateClientAsync(int id, ClientCreateDto dto)
        {
            var client = await _context.Client
                .Include(c => c.Numbers)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return (false, "No se encontró el cliente.");

            if (!string.IsNullOrWhiteSpace(dto.DocumentIdentificationNumber))
                client.DocumentIdentificationNumber = dto.DocumentIdentificationNumber;

            if (!string.IsNullOrWhiteSpace(dto.Names))
                client.Names = dto.Names;

            if (dto.Email != null) // permite limpiar correo si viene vacío
                client.Email = dto.Email;

            if (dto.DocumentIdentificationType != null)
                client.DocumentIdentificationType = dto.DocumentIdentificationType;

            if (dto.Gender != null)
                client.Gender = dto.Gender;

            client.UpdatedAt = DateTime.Now;

            if (dto.Numbers != null)
            {
                // Validar que no haya más de 1 principal
                if (dto.Numbers.Count(x => x.IsPrimary) > 1)
                    return (false, "Solo puede existir un número principal.");

                // Si hay números y ninguno es principal, marcamos el primero
                if (dto.Numbers.Count > 0 && !dto.Numbers.Any(x => x.IsPrimary))
                    dto.Numbers[0].IsPrimary = true;

                var oldNumbers = await _context.ClientNumbers
                    .Where(x => x.ClientId == client.Id)
                    .ToListAsync();

                if (oldNumbers.Any())
                    _context.ClientNumbers.RemoveRange(oldNumbers);

                var newNumbers = dto.Numbers.Select(n => new ClientNumbers
                {
                    ClientId = client.Id,
                    ContactName = n.ContactName,
                    Type = (ContactTypeEnum)n.Type, 
                    Number = n.Number,
                    IsPrimary = n.IsPrimary
                }).ToList();

                await _context.ClientNumbers.AddRangeAsync(newNumbers);
            }

            if (dto.Addresses != null)
            {
                if (dto.Addresses.Count(x => x.IsPrimary) > 1)
                    return (false, "Solo puede existir una dirección principal.");

                if(dto.Addresses.Count > 0 && !dto.Addresses.Any(x => x.IsPrimary))
                    dto.Addresses[0].IsPrimary = true;

                var oldAddress = await _context.ClientAddresses
                    .Where(x => x.ClientId == client.Id)
                    .ToListAsync();

                if(oldAddress.Any())
                    _context.ClientAddresses.RemoveRange(oldAddress);

                var newAddress = dto.Addresses.Select(n => new ClientAddress
                {
                    ClientId = client.Id,
                    AddressName = n.AddressName,
                    Address = n.Address,
                    IsPrimary = n.IsPrimary
                }).ToList();

                await _context.ClientAddresses.AddRangeAsync(newAddress);
            }
                

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
                Names = dto.Names,
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


            if (dto.Numbers != null && dto.Numbers.Count > 0)
            {
                if (dto.Numbers.Count(x => x.IsPrimary) > 1)
                    return (false, "Solo puede existir un número principal.");

                if (!dto.Numbers.Any(x => x.IsPrimary))
                    dto.Numbers[0].IsPrimary = true;

                var numbers = dto.Numbers.Select(n => new ClientNumbers
                {
                    ClientId = client.Id,
                    ContactName = n.ContactName,
                    Type = (ContactTypeEnum)n.Type,
                    Number = n.Number,
                    IsPrimary = n.IsPrimary
                }).ToList();

                _context.ClientNumbers.AddRange(numbers);
                await _context.SaveChangesAsync();
            }

            if(dto.Addresses != null && dto.Addresses.Count > 0)
            {
                if (dto.Addresses.Count(x => x.IsPrimary) > 1)
                    return (false, "Solo puede existir una dirección principal.");

                if(!dto.Addresses.Any(x => x.IsPrimary))
                    dto.Addresses[0].IsPrimary = true;

                var address = dto.Addresses.Select(n => new ClientAddress
                {
                    ClientId = client.Id,
                    AddressName = n.AddressName,
                    Address = n.Address,
                    IsPrimary = n.IsPrimary
                }).ToList();

                _context.ClientAddresses.AddRange(address);
                await _context.SaveChangesAsync();
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

        public async Task<(bool Success, string Message)> CreateClientAddress(int id, ClientAddress dto)
        {
            var exists = await _context.ClientAddresses.AnyAsync(c =>
                c.Address == dto.Address);

            if (exists)
                return (false, "Ya existe la dirección en la BD.");

            var client = new ClientAddress
            {
                ClientId = id,
                Address = dto.Address.Trim(),
                AddressName = dto.Address.Trim()
            };

            _context.ClientAddresses.Add(client);
            await _context.SaveChangesAsync();

            return (true, "Numero agregado correctamente.");
        }

        public async Task SaveClientNumbersAsync(int clientId, List<ClientContactCreateDto> numbers)
        {
            // 1) Borrar anteriores
            var oldNumbers = await _context.ClientNumbers
                .Where(x => x.ClientId == clientId)
                .ToListAsync();

            if (oldNumbers.Any())
            {
                _context.ClientNumbers.RemoveRange(oldNumbers);
            }

            // Si no enviaron números, guardamos vacío
            if (numbers == null || numbers.Count == 0)
            {
                await _context.SaveChangesAsync();
                return;
            }

            // 2) Validar Primary (solo uno)
            if (numbers.Count(x => x.IsPrimary) > 1)
                throw new Exception("Solo puede existir un número principal.");

            // Si ninguno es primary, asignamos el primero
            if (!numbers.Any(x => x.IsPrimary))
                numbers[0].IsPrimary = true;

            // 3) Insertar nuevos
            var newNumbers = numbers.Select(n => new ClientNumbers
            {
                ClientId = clientId,
                ContactName = n.ContactName,
                Type = (ContactTypeEnum)n.Type,
                Number = n.Number,
                IsPrimary = n.IsPrimary
            }).ToList();

            await _context.ClientNumbers.AddRangeAsync(newNumbers);
            await _context.SaveChangesAsync();
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
                IsActive = true
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
                        ClientId = ch.ClientId
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

    }
}
