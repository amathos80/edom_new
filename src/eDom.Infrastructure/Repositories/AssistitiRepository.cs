using eDom.Core.Entities;
using eDom.Infrastructure.Data;
using eDom.Infrastructure.Repositories;

public class AssistitiRepository(HctDbContext context) : Repository<VistaAssistito>(context), IAssistitiRepository
{

}