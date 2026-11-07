namespace SportskaLiga.Domain.Enums;

/// <summary>Ishod brisanja zapisa kroz repozitorij; kontroleri po njemu biraju poruku ili HTTP kod.</summary>
public enum DeleteResult
{
    /// <summary>Zapis je obrisan.</summary>
    Deleted,

    /// <summary>Zapis s tim Id-em ne postoji.</summary>
    NotFound,

    /// <summary>Zapis se ne može obrisati jer ga drugi zapisi referenciraju (npr. momčad s igračima).</summary>
    InUse
}
