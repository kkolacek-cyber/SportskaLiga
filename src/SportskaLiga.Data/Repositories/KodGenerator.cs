using System.Security.Cryptography;

namespace SportskaLiga.Data.Repositories;

/// <summary>Generira kratke kodove prijave bez znakova koji se lako brkaju (0/O, 1/I).</summary>
internal static class KodGenerator
{
    private const string Abeceda = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int Duljina = 8;

    // Kriptografski generator slučajnih brojeva: kodovi se ne mogu pogoditi niti predvidjeti.
    public static string Novi()
        => string.Create(Duljina, 0, static (znakovi, _) =>
        {
            for (var i = 0; i < znakovi.Length; i++)
            {
                znakovi[i] = Abeceda[RandomNumberGenerator.GetInt32(Abeceda.Length)];
            }
        });
}
