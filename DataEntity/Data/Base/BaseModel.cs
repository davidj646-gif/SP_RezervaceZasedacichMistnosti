using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DataEntity
{
    public abstract class BaseModel : IDataErrorInfo
    {

        #region "validace"

        // Chyba celého objektu – WPF ji čte jen při validaci celého řádku (BindingGroup).
        // Nesmí vyhazovat výjimku, jinak taková validace shodí aplikaci.
        string IDataErrorInfo.Error => string.Empty;

        string IDataErrorInfo.this[string propertyName] => OnValidate(propertyName);



        protected virtual string OnValidate(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Neplatný název vlastnosti", nameof(propertyName));

            var property = GetType().GetProperty(propertyName);
            if (property == null)
                return string.Empty;

            var value = property.GetValue(this, null);
            var results = new List<ValidationResult>(1);

            var context = new ValidationContext(this, null, null) { MemberName = propertyName };

            var result = Validator.TryValidateProperty(value, context, results);

            if (!result)
            {
                return results.First().ErrorMessage ?? string.Empty;
            }

            return string.Empty;
        }

        /// <summary>Vrátí všechny validační výsledky pro celý objekt (všechny vlastnosti).</summary>
        protected IReadOnlyList<ValidationResult> ValidateAll()
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(this);
            Validator.TryValidateObject(this, context, results, validateAllProperties: true);
            return results;
        }


        /// <summary>Počet chyb dle DataAnnotations.</summary>
        public int ErrorsCount => ValidateAll().Count;

        /// <summary>True, pokud objekt nemá žádné validační chyby.</summary>
        public bool IsValid => ErrorsCount == 0;



        #endregion

        // Hodnotu generuje SQL Server (rowversion) – EF ji při INSERTu neposílá
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public DateTime DatumVytvoreni { get; set; } = DateTime.Now;

    }
}