using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Admin.Models;

/// <summary>Podaci s obrasca za prijavu u administraciju.</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "E-mail je obavezan.")]
    [EmailAddress(ErrorMessage = "Unesite ispravnu e-mail adresu.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    [DataType(DataType.Password)]
    [Display(Name = "Lozinka")]
    public string Lozinka { get; set; } = string.Empty;

    [Display(Name = "Zapamti me")]
    public bool ZapamtiMe { get; set; }

    // Adresa koju je korisnik tražio prije preusmjeravanja na prijavu; nakon prijave vraća se na nju.
    public string? ReturnUrl { get; set; }
}
