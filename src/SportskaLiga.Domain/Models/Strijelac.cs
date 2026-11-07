namespace SportskaLiga.Domain.Models;

/// <summary>Red liste strijelaca. Igrači s istim brojem golova dijele poziciju (1., 2., 2., 4.).</summary>
public record Strijelac(int Pozicija, int IgracId, string Igrac, int MomcadId, string Momcad, int Golovi);
