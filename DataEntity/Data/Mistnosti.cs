using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataEntity.Data
{
    [AddINotifyPropertyChangedInterface]
    [Table("Mistnosti")]
    public class Mistnosti : Base.BaseModel
    {
        [Key]
        public int MistnostiId { get; set; }

        [Required(ErrorMessage = "Název místnosti je povinné pole")]
        [StringLength(100, ErrorMessage = "Název místnosti nesmí být delší než 100 znaků")]
        public string NazevMistnosti { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kapacita je povinné pole")]
        [Range(1, int.MaxValue, ErrorMessage = "Kapacita musí být větší než 0")] //Integritní omezení pro kapacitu místnosti -> Kapacita zasedací místnosti musí být větší než 0
        public int Kapacita { get; set; }

        public virtual ICollection<Rezervace> Rezervace { get; set; } = new List<Rezervace>();
        public override string ToString() => $"{NazevMistnosti} (kapacita: {Kapacita})";
    }
}
