using eDom.Core.Entities;
using eDom.Core.Models;

namespace eDom.Core.Interfaces;

public interface ISearch<T> 
{
    
}

public interface IAssistitiEPazientiRepository : ISearch<VistaRicercaPazienteAssistito>
{
    Task<PagedResult<VistaRicercaPazienteAssistito>> SearchAsync(
        string? cognome,
        string? nome,
        string? codiceFiscale,
        DateOnly? dataNascita,
        int page, 
        int pageSize, 
        CancellationToken ct = default);
}