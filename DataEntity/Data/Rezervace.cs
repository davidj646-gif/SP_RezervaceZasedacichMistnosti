using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using System.Text;

namespace DataEntity.Data
{
    [AddINotifyPropertyChangedInterface]
    [Table("Rezervace")]
    public class Rezervace : Base.BaseModel
    {
        [Key]
        public int RezervaceId { get; set; }

        public int ZamestnanciId { get; set; }

        [ForeignKey(nameof(ZamestnanciId))]
        public virtual Zamestnanci Zamestnanec { get; set; } = null!;

        public int MistnostiId { get; set; }

        [ForeignKey(nameof(MistnostiId))]
        public virtual Mistnosti Mistnost { get; set; } = null!;

        [Column(TypeName = "date")]
        public DateTime DatumVytvoreni { get; set; } = DateTime.Now;

        [Column(TypeName = "date")]
        public DateTime CasOd { get; set; } = DateTime.Now;

        [Column(TypeName = "date")]
        public DateTime CasDo { get; set; } = DateTime.Now.AddHours(1);

        [Required(ErrorMessage = "Účel rezervace je povinné pole")]
        [StringLength(500, ErrorMessage = "Účel rezervace nesmí být delší než 500 znaků")]
        public string Ucel { get; set; } = string.Empty;

        [NotMapped]
        public Enums.RezervaceStav Stav => DateTime.Now < CasOd ? Enums.RezervaceStav.Budouci : DateTime.Now >= CasOd && DateTime.Now <= CasDo ? Enums.RezervaceStav.Probihajici : Enums.RezervaceStav.Uskutecnena;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) // Integritní omezení pro rezervace (např. max 8 hodin, CasDo > CasOd)
        {
            if (CasDo <= CasOd) yield return new("Ukončení musí být pozdější než zahájení.", [nameof(CasDo)]);
            if (CasDo - CasOd > TimeSpan.FromHours(8)) yield return new("Rezervace smí trvat nejvýše 8 hodin.", [nameof(CasDo)]);
        }
    }
}
