using Microsoft.EntityFrameworkCore;
using Pisco.Domain;

namespace Pisco.Data;

public class PiscoDbContext(DbContextOptions<PiscoDbContext> options) : DbContext(options)
{
    public const string Schema = "pisco_config";

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Config> Config => Set<Config>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Workspace>(ws =>
        {
            ws.ToTable("workspaces", Schema);
            ws.HasKey(w => w.Id);

            ws.Property(w => w.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            ws.Property(w => w.Name).HasColumnName("name").HasMaxLength(Workspace.NameLengthMax).IsRequired();
            ws.Property(w => w.IsPublic).HasColumnName("is_public");
        });

        model.Entity<Config>(config =>
        {
            config.ToTable("config", Schema);
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
    }
}
