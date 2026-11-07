using SportskaLiga.Domain.Enums;

namespace SportskaLiga.Domain.Interfaces;

/// <summary>Osnovne CRUD operacije nad entitetom s cjelobrojnim ključem <c>Id</c>. Svaka izmjena se odmah sprema.</summary>
public interface IRepository<T> where T : class
{
    /// <summary>Zapis po Id-u (praćen, pa se može mijenjati i spremiti s <c>UpdateAsync</c>) ili null.</summary>
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Svi zapisi (za male tablice kao što su suci).</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Dodaje novi zapis i odmah ga sprema.</summary>
    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>Sprema izmjene zapisa.</summary>
    Task UpdateAsync(T entity, CancellationToken ct = default);

    /// <summary>Briše zapis; ako ga drugi zapisi referenciraju, vraća <see cref="DeleteResult.InUse"/> umjesto da baci iznimku.</summary>
    Task<DeleteResult> DeleteAsync(int id, CancellationToken ct = default);
}
