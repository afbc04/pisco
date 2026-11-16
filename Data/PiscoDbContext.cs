using Microsoft.EntityFrameworkCore;
using Pisco.Domain;

namespace Pisco.Data;

public class PiscoDbContext(DbContextOptions<PiscoDbContext> options) : DbContext(options)
{
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Config> Config => Set<Config>();
    public DbSet<Wallet> Wallets => Set<Wallet>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Workspace>(ws =>
        {
            ws.ToTable("workspaces");
            ws.HasKey(w => w.Id);

            ws.Property(w => w.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            ws.Property(w => w.Name).HasColumnName("name").HasMaxLength(Workspace.NameLengthMax).IsRequired();
            ws.Property(w => w.IsPublic).HasColumnName("is_public");
        });

        model.Entity<Config>(config =>
        {
            config.ToTable("config");
            config.HasKey(c => c.Id);

            config.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
            config.Property(c => c.Username).HasColumnName("username").HasMaxLength(Domain.Config.UsernameLengthMax).IsRequired();
            config.Property(c => c.PasswordHash).HasColumnName("password_hash").IsRequired();
            config.Property(c => c.PasswordSalt).HasColumnName("password_salt").IsRequired();
            config.Property(c => c.LandingWorkspaceId).HasColumnName("landing_workspace_id");
            config.Property(c => c.LastWorkspaceId).HasColumnName("last_workspace_id");

            config.HasOne<Workspace>().WithMany().HasForeignKey(c => c.LandingWorkspaceId).OnDelete(DeleteBehavior.Restrict);
            config.HasOne<Workspace>().WithMany().HasForeignKey(c => c.LastWorkspaceId).OnDelete(DeleteBehavior.Restrict);
        });

        model.Entity<Wallet>(wallet =>
        {
            wallet.ToTable("wallets");
            wallet.HasKey(w => w.ID);

            wallet.Property(w => w.ID).HasColumnName("id").UseIdentityAlwaysColumn();
            wallet.Property(w => w.Workspace).HasColumnName("workspace_id");
            wallet.Property(w => w.Name).HasColumnName("name").HasMaxLength(Wallet.NameLengthMax).IsRequired();
            wallet.Property(w => w.Color).HasColumnName("color").HasMaxLength(Wallet.ColorLengthMax).IsFixedLength().IsRequired();
            wallet.Property(w => w.Icon).HasColumnName("icon").HasMaxLength(Wallet.IconLengthMax).IsRequired();
            wallet.Property(w => w.InitialBalance).HasColumnName("initial_balance");
            wallet.Property(w => w.Active).HasColumnName("active");
            wallet.Property(w => w.CreationDate).HasColumnName("creation_date");
            wallet.Property(w => w.UpdatedDate).HasColumnName("updated_date");
            wallet.Property(w => w.InactiveDate).HasColumnName("inactive_date");
            wallet.Property(w => w.DeletedDate).HasColumnName("deleted_date");
            wallet.Property(w => w.Status).HasColumnName("status");
            wallet.Ignore(w => w.IsDeleted);
            wallet.Ignore(w => w.IsDraft);
            wallet.Ignore(w => w.IsPublished);

            wallet.HasOne<Workspace>().WithMany().HasForeignKey(w => w.Workspace).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
