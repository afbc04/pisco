using Microsoft.EntityFrameworkCore;
using Pisco.Domain;

namespace Pisco.Data;

public class PiscoDbContext(DbContextOptions<PiscoDbContext> options) : DbContext(options)
{
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<Config> Config => Set<Config>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();

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

        model.Entity<Category>(category =>
        {
            category.ToTable("categories");
            category.HasKey(c => c.ID);

            category.Property(c => c.ID).HasColumnName("id").UseIdentityAlwaysColumn();
            category.Property(c => c.Workspace).HasColumnName("workspace_id");
            category.Property(c => c.Flow).HasColumnName("flow");
            category.Property(c => c.Kind).HasColumnName("kind");
            category.Property(c => c.Parent).HasColumnName("parent_id");
            category.Property(c => c.Name).HasColumnName("name").HasMaxLength(Category.NameLengthMax).IsRequired();
            category.Property(c => c.Icon).HasColumnName("icon").HasMaxLength(Category.IconLengthMax).IsRequired();
            category.Property(c => c.Active).HasColumnName("active");
            category.Property(c => c.CreationDate).HasColumnName("creation_date");
            category.Property(c => c.UpdatedDate).HasColumnName("updated_date");
            category.Property(c => c.InactiveDate).HasColumnName("inactive_date");
            category.Property(c => c.DeletedDate).HasColumnName("deleted_date");
            category.Property(c => c.Status).HasColumnName("status");
            category.Ignore(c => c.IsDeleted);
            category.Ignore(c => c.IsDraft);
            category.Ignore(c => c.IsPublished);
            category.Ignore(c => c.IsSubcategory);

            category.HasOne<Workspace>().WithMany().HasForeignKey(c => c.Workspace).OnDelete(DeleteBehavior.Cascade);
            category.HasOne<Category>().WithMany().HasForeignKey(c => c.Parent).OnDelete(DeleteBehavior.Restrict);
        });

        model.Entity<Tag>(tag =>
        {
            tag.ToTable("tags");
            tag.HasKey(t => t.ID);

            tag.Property(t => t.ID).HasColumnName("id").UseIdentityAlwaysColumn();
            tag.Property(t => t.Workspace).HasColumnName("workspace_id");
            tag.Property(t => t.Name).HasColumnName("name").HasMaxLength(Tag.NameLengthMax).IsRequired();
            tag.Property(t => t.Color).HasColumnName("color").HasMaxLength(Tag.ColorLength).IsFixedLength().IsRequired();

            tag.HasOne<Workspace>().WithMany().HasForeignKey(t => t.Workspace).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
