using DataEntity.Data;
using DataEntity.Data.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DataEntity
{
    public class RezervaceDbContext : DbContext
    {
        public DbSet<Mistnosti> Mistnosti => Set<Mistnosti>();
        public DbSet<Zamestnanci> Zamestnanci => Set<Zamestnanci>();
        public DbSet<Rezervace> Rezervace => Set<Rezervace>();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {optionsBuilder.
                    UseSqlServer(
                        "Data Source=(localdb)\\MSSQLLocalDB;" +
                        "Database=RezervaceZasedacichMistnosti" +
                        "Integrated Security=true" +
                        "TrustServerCertificate=true")
                    .UseLazyLoadingProxies();
            }
        }
        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Mistnosti>().ToTable("Mistnosti", t => t.HasCheckConstraint("CK_Mistnosti_Kapacita", "[Kapacita] > 0"));
            b.Entity<Zamestnanci>().ToTable("Zamestnanci");
            b.Entity<Rezervace>().ToTable("Rezervace", t => {
                t.UseSqlOutputClause(false); // SQL Server tabulka má integrační triggery.
                t.HasCheckConstraint("CK_Rezervace_Interval", Database.IsSqlServer() ? "[CasDo] > [CasOd] AND [CasDo] <= DATEADD(hour, 8, [CasOd])" : "[CasDo] > [CasOd] AND julianday([CasDo]) <= julianday([CasOd], '+8 hours')");
                t.HasCheckConstraint("CK_Rezervace_Ucel", Database.IsSqlServer() ? "LEN(LTRIM(RTRIM([Ucel]))) > 0" : "length(trim([Ucel])) > 0");
            });
            b.Entity<Rezervace>().HasOne(x => x.Mistnost).WithMany(x => x.Rezervace).HasForeignKey(x => x.MistnostiId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<Rezervace>().HasOne(x => x.Zamestnanec).WithMany(x => x.Rezervace).HasForeignKey(x => x.ZamestnanciId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<Rezervace>().HasIndex(x => new { x.MistnostiId, x.CasOd, x.CasDo });
            b.Entity<Rezervace>().HasIndex(x => new { x.ZamestnanciId, x.CasOd, x.CasDo });
            b.Entity<Rezervace>().HasIndex(x => x.DatumVytvoreni);
            // SQLite slouží jen pro automatické testy aplikačních pravidel.
            if (!Database.IsSqlServer())
            {
                b.Entity<Mistnosti>().Ignore(x => x.RowVersion);
                b.Entity<Zamestnanci>().Ignore(x => x.RowVersion);
                b.Entity<Rezervace>().Ignore(x => x.RowVersion);
            }
        }
            private void ValidateChanges()
            {
                ChangeTracker.DetectChanges();
                var now = DateTime.UtcNow;
                foreach (var e in ChangeTracker.Entries<BaseModel>().Where(e => e.State is EntityState.Added or EntityState.Modified))
                {
                    var errors = e.Entity.ValidateAll();
                    if (errors.Count > 0) throw new ValidationException(string.Join("\n", errors.Select(x => x.ErrorMessage)));
                }
                var entries = ChangeTracker.Entries<Rezervace>().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
                var changedIds = entries.Where(e => e.State != EntityState.Added).Select(e => e.Entity.RezervaceId).ToArray();
                var pending = entries.Where(e => e.State != EntityState.Deleted).Select(e => e.Entity).ToList();
                foreach (var e in entries)
                {
                    if (e.State != EntityState.Added && e.OriginalValues.GetValue<DateTime>(nameof(DataEntity.Data.Rezervace.CasOd)) <= now)
                        throw new ValidationException("Probíhající a uskutečněné rezervace nelze měnit ani mazat.");
                    if (e.State == EntityState.Deleted) continue;
                    var x = e.Entity;
                    if (x.CasOd <= now) throw new ValidationException("Zahájení rezervace musí být v budoucnosti.");
                    if (e.State == EntityState.Modified && e.Property(x => x.DatumVytvoreni).IsModified)
                        throw new ValidationException("Datum vytvoření rezervace nelze změnit.");
                    if (Rezervace.AsNoTracking().Any(y => !changedIds.Contains(y.RezervaceId) && y.CasOd < x.CasDo && x.CasOd < y.CasDo
                            && (y.MistnostiId == x.MistnostiId || y.ZamestnanciId == x.ZamestnanciId))
                        || pending.Any(y => !ReferenceEquals(x, y) && PrekryvaSe(x.CasOd, x.CasDo, y.CasOd, y.CasDo)
                            && (y.MistnostiId == x.MistnostiId || y.ZamestnanciId == x.ZamestnanciId)))
                        throw new ValidationException("Termín se překrývá s rezervací místnosti nebo zaměstnance.");
                }
            }
                public static bool PrekryvaSe(DateTime od, DateTime doCas, DateTime jinyOd, DateTime jinyDo) => od < jinyDo && jinyOd < doCas;
                public override int SaveChanges(bool acceptAllChangesOnSuccess)
                {
                    ValidateChanges();
                    return base.SaveChanges(acceptAllChangesOnSuccess);
                }
                public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
                {
                    ValidateChanges();
                    return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                }
                public IQueryable<Mistnosti> DostupneMistnosti(DateTime od, DateTime doCas, int kapacita, int vynechatId = 0) =>
                    Mistnosti.AsNoTracking().Where(m => m.Kapacita >= kapacita && !Rezervace.Any(r => r.RezervaceId != vynechatId && r.MistnostiId == m.MistnostiId && r.CasOd < doCas && od < r.CasDo))
                        .OrderByDescending(m => m.Kapacita).ThenBy(m => m.NazevMistnosti);
            }
}

