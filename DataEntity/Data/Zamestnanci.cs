using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataEntity.Data
{
    [AddINotifyPropertyChangedInterface]
    [Table("Zamestnanci")]
    public class Zamestnanci : Base.BaseModel
    {
        [Key]
        public int ZamestnanciId { get; set; }

        [Required(ErrorMessage = "Jméno je povinné pole")]
        [StringLength(100, ErrorMessage = "Jméno nesmí být delší než 100 znaků")]
        public string Jmeno { get; set; } = string.Empty;

        [Required(ErrorMessage = "Příjmení je povinné pole")]
        [StringLength(100, ErrorMessage = "Příjmení nesmí být delší než 100 znaků")]
        public string Prijmeni { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je povinné pole")]
        [StringLength(100, ErrorMessage = "Email nesmí být delší než 100 znaků")]
        [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
        public string Email { get; set; } = string.Empty;

        public virtual ICollection<Rezervace> Rezervace { get; set; } = new List<Rezervace>();

        [NotMapped] public string CeleJmeno => $"{Jmeno} {Prijmeni}";
        public override string ToString() => $"{CeleJmeno} – {Email}";
    }
}
