using System.ComponentModel;

namespace DataEntity
{
    public static class Enums
    {
        public enum MistnostTyp
        {
            [Description("Malá")]
            Mala = 0,

            [Description("Střední")]
            Velka = 1,

            [Description("Velká")]
            Delena = 2
        }

        public enum RezervaceStav
        {
            [Description("Budoucí")]
            Zaskladneno = 0,

            [Description("Aktivní")]
            Vyskladneno = 1,

            [Description("Probíhající")]
            Doprava = 2
        }


    }
}