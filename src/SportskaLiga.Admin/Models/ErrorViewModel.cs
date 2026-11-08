namespace SportskaLiga.Admin.Models;

/// <summary>Podaci za stranicu greške: identifikator zahtjeva koji se može javiti podršci.</summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
