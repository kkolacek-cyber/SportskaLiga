using Microsoft.EntityFrameworkCore;
using Npgsql;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;

namespace SportskaLiga.Data.Repositories;

/// <summary>
/// Opći repozitorij: osnovni CRUD nad bilo kojim entitetom s cjelobrojnim Id-em. Posebni repozitoriji ga nasljeđuju
/// i dodaju vlastite upite. Svaka izmjena (Add, Update, Delete) odmah poziva SaveChanges.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    // PostgreSQL SQLSTATE 23503 = foreign_key_violation
    private const string ForeignKeyViolation = "23503";

    // Dostupni nasljednicima: kontekst i DbSet entiteta kojim se bavi ovaj repozitorij.
    protected readonly SportskaLigaDbContext Context;
    protected readonly DbSet<T> Set;

    public Repository(SportskaLigaDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    // FindAsync prvo gleda već učitane (praćene) entitete pa tek onda ide u bazu.
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await Set.FindAsync([id], ct);

    // AsNoTracking: samo za čitanje, EF ne prati promjene pa je brže.
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        // Entitet učitan iz ovog contexta je već praćen; odvojeni (npr. složen iz ViewModela) treba označiti.
        if (Context.Entry(entity).State == EntityState.Detached)
        {
            Set.Update(entity);
        }

        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task<DeleteResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await Set.FindAsync([id], ct);
        if (entity is null)
        {
            return DeleteResult.NotFound;
        }

        Set.Remove(entity);
        try
        {
            await Context.SaveChangesAsync(ct);
            return DeleteResult.Deleted;
        }
        // Baza je odbila brisanje jer zapis još netko koristi (strani ključ): ne bacamo iznimku nego javljamo "InUse".
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: ForeignKeyViolation })
        {
            Context.Entry(entity).State = EntityState.Unchanged;
            return DeleteResult.InUse;
        }
    }
}
